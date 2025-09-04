using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using LawnCare.Shared.OpenTelemetry;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LawnCare.Shared.EntityFramework;

public static class MigrateDbContextExtensions
{
	private static readonly string ActivitySourceName = "DbMigrations";
	private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

	public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
		where TContext : DbContext
		=> services.AddMigration<TContext>((_, _) => Task.CompletedTask);

	public static IServiceCollection AddMigration<TContext>(this IServiceCollection services, Func<TContext, IServiceProvider, Task> seeder)
		where TContext : DbContext
	{
		// Enable migration tracing
		services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

		return services.AddHostedService(sp => new MigrationHostedService<TContext>(sp, seeder));
	}

	public static IServiceCollection AddMigration<TContext, TDbSeeder>(this IServiceCollection services)
		where TContext : DbContext
		where TDbSeeder : class, IDbSeeder<TContext>
	{
		services.AddScoped<IDbSeeder<TContext>, TDbSeeder>();
		return services.AddMigration<TContext>((context, sp) => sp.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context));
	}

	private static async Task  MigrateDbContextAsync<TContext>(this IServiceProvider services, Func<TContext, IServiceProvider, Task> seeder) where TContext : DbContext
	{
		using var scope = services.CreateScope();
		var scopeServices = scope.ServiceProvider;
		var logger = scopeServices.GetRequiredService<ILogger<TContext>>();
		var context = scopeServices.GetService<TContext>()!;

		using var activity = ActivitySource.StartActivity($"Migration operation {typeof(TContext).Name}");

		try
		{
			logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(() => InvokeSeeder(seeder, context, scopeServices));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);

			activity?.SetExceptionTags(ex);

			throw;
		}
	}

	private static async Task InvokeSeeder<TContext>(Func<TContext, IServiceProvider, Task> seeder, TContext context, IServiceProvider services)
		where TContext : DbContext
	{
		using var activity = ActivitySource.StartActivity($"Migrating {typeof(TContext).Name}");

		try
		{
			await context.Database.MigrateAsync();
			await seeder(context, services);
		}
		catch (Exception ex)
		{
			activity?.SetExceptionTags(ex);

			throw;
		}
	}

	private class MigrationHostedService<TContext>(IServiceProvider serviceProvider, Func<TContext, IServiceProvider, Task> seeder)
		: BackgroundService where TContext : DbContext
	{
		public override Task StartAsync(CancellationToken cancellationToken)
		{
			return serviceProvider.MigrateDbContextAsync(seeder);
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return Task.CompletedTask;
		}
	}
// 	
// 	//public static class ValueConversionExtensions
// 	//{
// 		public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder, JsonSerializerOptions options) where T : class, new()
// 		{
// 			
// 			
// 			ValueConverter<T, string> converter = new ValueConverter<T, string>
// 			(
// 				v => System.Text.Json.JsonSerializer.Serialize(v, options),
// 				v => System.Text.Json.JsonSerializer.Deserialize<T>(v, options) ?? new T()
// 			);
//
// 			ValueComparer<T> comparer = new ValueComparer<T>
// 			(
// 				(l, r) => System.Text.Json.JsonSerializer.Serialize(l, options) == System.Text.Json.JsonSerializer.Serialize(r, options),
// 				v => v == null ? 0 : JsonSerializer.Serialize(v,options).GetHashCode(),
// 				v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v,options), options )!
// 			);
//
// 			propertyBuilder.HasConversion(converter);
// 			propertyBuilder.Metadata.SetValueConverter(converter);
// 			propertyBuilder.Metadata.SetValueComparer(comparer);
// 			propertyBuilder.HasColumnType("jsonb");
//
// 			return propertyBuilder;
// 		}
// //	}
	
}

public interface IDbSeeder<in TContext> where TContext : DbContext
{
	Task SeedAsync(TContext context);
}


using FluentValidation;

using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.CoreApi.UseCases;
using LawnCare.JobApi.Infrastructure.Database;
using LawnCare.Shared.Endpoints;
using LawnCare.Shared.EntityFramework;
using LawnCare.Shared.OpenTelemetry;
using LawnCare.Shared.Pipelines;
using LawnCare.Shared.ProjectSetup;

using MassTransit;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddCors();
builder.Services.AddEndpoints(typeof(Program).Assembly);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssemblyContaining<Program>();
	cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
	cfg.AddOpenBehavior(typeof(HandlerBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
builder.Services.AddMassTransit(x =>
{
	//x.AddConsumer<MoveJobToPendingCommandConsumer>(typeof(MoveJobToPendingCommandConsumerDefinition));
	x.SetKebabCaseEndpointNameFormatter();
	x.UsingRabbitMq((context, configuration) =>
	{
		configuration.Host(builder.Configuration.GetConnectionString("rabbitmq"));
		configuration.ConfigureEndpoints(context);
	});
});
// Register MassTransit.Mediator, good luck not confusing this with the other one
builder.Services.AddMediator(cfg =>
{
	cfg.AddConsumers(typeof(Program).Assembly);
});

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();
//builder.Services.AddTransient<IJobApplicationService, JobApplicationService>();
//builder.Services.AddTransient<JobDomainService, JobDomainService>();
//builder.Services.AddTransient<IJobRepository, JobRepository>();
//builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<JobMappingService, JobMappingService>();
builder.Services.AddDbContext<CoreDbContext>(dbContextOptionsBuilder =>
{
	dbContextOptionsBuilder.UseNpgsql(
			builder.Configuration.GetConnectionString("job-connection"))
		.UseSnakeCaseNamingConvention();
});
builder.Services.AddMigration<CoreDbContext>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

// var summaries = new[]
// {
// 	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };
//
// app.MapGet("/weatherforecast", () =>
// 	{
// 		var forecast = Enumerable.Range(1, 5).Select(index =>
// 				new WeatherForecast
// 				(
// 					DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
// 					Random.Shared.Next(-20, 55),
// 					summaries[Random.Shared.Next(summaries.Length)]
// 				))
// 			.ToArray();
// 		return forecast;
// 	})
// 	.WithName("GetWeatherForecast");
app.UseCors(x => x
    .WithOrigins("https://localhost:7000", "http://localhost:5000", "https://localhost:5001)") // ManagementUI URLs
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
app.UseRouting();
app.MapEndpoints();
app.Run();






// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
// 	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
using FluentValidation;

using JobService.Infrastructure.Persistence;

using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.Repositories;
using LawnCare.JobApi.Domain.Services;
using LawnCare.JobApi.Infrastructure.Database;
using LawnCare.JobApi.UseCases;
using LawnCare.Shared.Endpoints;
using LawnCare.Shared.EntityFramework;
using LawnCare.Shared.MessageContracts;
using LawnCare.Shared.OpenTelemetry;
using LawnCare.Shared.Pipelines;
using LawnCare.Shared.ProjectSetup;

using MassTransit;
using MassTransit.Mediator;

using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddCors();
builder.Services.AddEndpoints(typeof(Program).Assembly);

// Add services to the container.
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
	x.AddConsumer<MoveJobToPendingCommandConsumer>(typeof(MoveJobToPendingCommandConsumerDefinition));
	x.SetKebabCaseEndpointNameFormatter();
	x.UsingRabbitMq((context, configuration) =>
	{
		configuration.Host(builder.Configuration.GetConnectionString("rabbitmq"));
		configuration.ConfigureEndpoints(context);
	});
});

// Register MassTransit.Mediator
builder.Services.AddMediator(cfg =>
{
	cfg.AddConsumers(typeof(Program).Assembly);
});

builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();
builder.Services.AddTransient<IJobApplicationService, JobApplicationService>();
builder.Services.AddTransient<JobDomainService, JobDomainService>();
builder.Services.AddTransient<IJobRepository, JobRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<JobDbContext>(dbContextOptionsBuilder =>
{
	dbContextOptionsBuilder.UseNpgsql(
			builder.Configuration.GetConnectionString("job-connection"))
		.UseSnakeCaseNamingConvention();
});
builder.Services.AddMigration<JobDbContext>();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
	{
		return Results.Ok();
	})
	.WithName("Test");

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseRouting();
app.MapEndpoints();
app.Run();
using System.Net.Mail;

using LawnCare.Communications;
using LawnCare.Communications.Consumers;
using LawnCare.Communications.Configuration;
//using LawnCare.CustomerApi.Infrastructure;
using LawnCare.Shared.EntityFramework;
using LawnCare.Shared.OpenTelemetry;
using LawnCare.Shared.ProjectSetup;
using MassTransit;
using Microsoft.EntityFrameworkCore;


//
var builder = WebApplication.CreateBuilder();
builder.Configuration
	.AddEnvironmentVariables();

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddMassTransit(x =>
{
	// Existing consumers
	x.AddConsumer<SendWelcomeEmailCommandConsumer>(typeof(SendWelcomeEmailCommandConsumerDefinition));
	x.AddConsumer<JobCreatedEmailConsumer>(typeof(JobCreatedEmailConsumerDefinition));
	x.AddConsumer<JobUpdatedEmailConsumer>(typeof(JobUpdatedEmailConsumerDefinition));
	x.AddConsumer<JobCompletedEmailConsumer>(typeof(JobCompletedEmailConsumerDefinition));

	x.SetKebabCaseEndpointNameFormatter();
	x.UsingRabbitMq((context, configuration) =>
	{
		configuration.Host(builder.Configuration.GetConnectionString("rabbitmq"));
		configuration.ConfigureEndpoints(context);

		// Configure global retry policy using centralized configuration
		configuration.UseMessageRetry(ResiliencyConfiguration.ConfigureRetryPolicy);
	});
});
builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();
builder.Services.AddSingleton<SmtpClient>(mail =>
{
	var uri = new Uri(builder.Configuration.GetConnectionString("maildev")!);
	var client = new SmtpClient(uri.Host, uri.Port);
	return client;
});
builder.Services.AddDbContext<EmailDbContext>(dbContextOptionsBuilder =>
{
	dbContextOptionsBuilder.UseNpgsql(
			builder.Configuration.GetConnectionString("communications-connection"))
		.UseSnakeCaseNamingConvention();
});
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddMigration<EmailDbContext>();
//builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapGet("/", () =>
	{
		return Results.Ok(DateTime.Now.Ticks);
	})
	.WithName("Check");

app.Run();

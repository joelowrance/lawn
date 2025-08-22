using System.Net.Mail;

using LawnCare.Communications;
//using LawnCare.CustomerApi.Infrastructure;
using LawnCare.Shared.EntityFramework;
using LawnCare.Shared.MessageContracts;
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
	x.AddConsumer<SendWelcomeEmailCommandConsumer>(typeof(SendWelcomeEmailCommandConsumerDefinition));
	x.SetKebabCaseEndpointNameFormatter();
	x.UsingRabbitMq((context, configuration) =>
	{
		configuration.Host(builder.Configuration.GetConnectionString("rabbitmq"));
		configuration.ConfigureEndpoints(context);
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
builder.Services.AddSingleton<IEmailService, EmailService>();
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

public class SendWelcomeEmailCommandConsumer : IConsumer<SendWelcomeEmailCommand>
{
	private readonly ILogger<ProcessCustomerCommand> _logger;
	private readonly IEmailService _emailService;

	private readonly IPublishEndpoint _publishEndpoint;
	//private readonly ICustomerService _customerService;

	public SendWelcomeEmailCommandConsumer(ILogger<ProcessCustomerCommand> logger, IEmailService emailService, IPublishEndpoint publishEndpoint)
	{
		_logger = logger;
		_emailService = emailService;
		_publishEndpoint = publishEndpoint;
	}

	public async Task Consume(ConsumeContext<SendWelcomeEmailCommand> context)
	{
		ArgumentNullException.ThrowIfNull(context);
		_logger.LogInformation("Received customer processing message for {CustomerEmail}", 
			context.Message.Customer.Email);

		await _emailService.SendWelcomeEmail(context.Message.Customer);
		await _publishEndpoint.Publish(new WelcomeEmailSentEvent(context.Message.TenantId, context.Message.EstimateId, context.Message.CustomerId));
		_logger.LogInformation("Successfully processed customer message for {CustomerEmail}", 
			context.Message.Customer.Email);
	}
}

internal class SendWelcomeEmailCommandConsumerDefinition : ConsumerDefinition<SendWelcomeEmailCommandConsumer>
{
	public SendWelcomeEmailCommandConsumerDefinition()
	{
		EndpointName = "communications-api";
		ConcurrentMessageLimit = 10;
        
	}

	protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SendWelcomeEmailCommandConsumer> consumerConfigurator,
		IRegistrationContext context)
	{
		endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
	}
}





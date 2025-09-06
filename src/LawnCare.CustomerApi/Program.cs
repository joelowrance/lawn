using FluentValidation;

using LawnCare.CustomerApi.Infrastructure;
using LawnCare.Shared.EntityFramework;
using LawnCare.Shared.MessageContracts;
using LawnCare.Shared.OpenTelemetry;
using LawnCare.Shared.Pipelines;
using LawnCare.Shared.ProjectSetup;

using MassTransit;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

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
	x.AddConsumer<ProcessCustomerCommandConsumer>(typeof(EstimateRequestConsumerDefinition));
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
builder.Services.AddDbContext<CustomerDbContext>(dbContextOptionsBuilder =>
{
	dbContextOptionsBuilder.UseNpgsql(
			builder.Configuration.GetConnectionString("customers-connection"))
		.UseSnakeCaseNamingConvention();
});
builder.Services.AddMigration<CustomerDbContext>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

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

public class ProcessCustomerCommandConsumer : IConsumer<ProcessCustomerCommand>
{
	private readonly ILogger<ProcessCustomerCommand> _logger;
	private readonly ICustomerService _customerService;

	public ProcessCustomerCommandConsumer(
		ILogger<ProcessCustomerCommand> logger,
		ICustomerService customerService)
	{
		_logger = logger;
		_customerService = customerService;
	}

	public async Task Consume(ConsumeContext<ProcessCustomerCommand> context)
	{
		ArgumentNullException.ThrowIfNull(context);
		_logger.LogInformation("Received customer processing message for {CustomerEmail}", 
			context.Message.Customer.Email);

		await _customerService.ProcessCustomerAsync(context.Message);

		_logger.LogInformation("Successfully processed customer message for {CustomerEmail}", 
			context.Message.Customer.Email);
	}
}

internal class EstimateRequestConsumerDefinition : ConsumerDefinition<ProcessCustomerCommandConsumer>
{
	public EstimateRequestConsumerDefinition()
	{
		EndpointName = "lawn-api";
		ConcurrentMessageLimit = 10;
        
	}

	protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<ProcessCustomerCommandConsumer> consumerConfigurator,
		IRegistrationContext context)
	{
		endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
	}
}


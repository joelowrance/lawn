using MassTransit;
using MediatR;
using FluentValidation;

using LawnCare.Shared;
using LawnCare.Shared.MessageContracts;
using LawnCare.Shared.OpenTelemetry;
using LawnCare.Shared.Pipelines;
using LawnCare.Shared.ProjectSetup;

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



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/estimate", async (IMediator mediator, FieldEstimate estimate) =>
{
	await mediator.Send(estimate);
	// needs a better result than just a status code
	return Results.Created();
});

app.MapGet("/", () =>
	{
		return Results.Ok();
	})
	.WithName("Test");

app.Run();



public record ProcessEstimateCommand(FieldEstimate Estimate) : IRequest;

public class ProcessEstimateCommandHandler : IRequestHandler<ProcessEstimateCommand>
{
	
	ILogger<ProcessEstimateCommandHandler> _logger;
	IPublishEndpoint _publishEndpoint;

	public ProcessEstimateCommandHandler(ILogger<ProcessEstimateCommandHandler> logger, IPublishEndpoint publishEndpoint)
	{
		_logger = logger;
		_publishEndpoint = publishEndpoint;
	}

	public async Task Handle(ProcessEstimateCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Queueing estimate: @{Estimate}", request.Estimate);

		var estimate = new EstimateReceived(
			GuidHelper.NewId(),
			GuidHelper.NewId(),
			new CustomerInfo
			(
				request.Estimate.CustomerEmail,
				request.Estimate.CustomerFirstName,
				request.Estimate.CustomerLastName,
				request.Estimate.CustomerHomePhone,
				request.Estimate.CustomerCellPhone,
				new Address(
					request.Estimate.CustomerAddress1,
					request.Estimate.CustomerAddress2,
					request.Estimate.CustomerAddress3,
					request.Estimate.CustomerCity,
					request.Estimate.CustomerState,
					request.Estimate.CustomerZip)
			),
			new JobDetails(
				request.Estimate.ScheduledDate,
				request.Estimate.EstimatedCost,
				request.Estimate.Description,
				request.Estimate.Services.ToArray()
			), "HARDCODED ESTIMATOR");
			
		
		await _publishEndpoint.Publish(estimate, cancellationToken);
	}
}


public class FieldEstimateValidator : AbstractValidator<FieldEstimate>
{
	public FieldEstimateValidator()
	{
		RuleFor(e => e.UserId).NotEmpty();
		RuleFor(e => e.TenantId).NotEmpty();
		RuleFor(e => e.CustomerFirstName).NotEmpty();
		RuleFor(e => e.CustomerLastName).NotEmpty();
		RuleFor(e => e.CustomerAddress1).NotEmpty();
		RuleFor(e => e.CustomerCity).NotEmpty();
		RuleFor(e => e.CustomerState).NotEmpty();
		RuleFor(e => e.CustomerZip).NotEmpty();
		RuleFor(e => e.ScheduledDate).Must(x => x.Date > DateTimeOffset.Now.AddDays(7))
			.WithMessage("Jobs must be scheduled at least 7 days in the future");
		RuleFor(e => e.CustomerHomePhone)
			.NotEmpty()
			.When(x => string.IsNullOrWhiteSpace(x.CustomerCellPhone))
			.WithMessage("Home phone or cell phone is required");
		RuleFor(e => e.CustomerCellPhone)
			.NotEmpty()
			.When(x => string.IsNullOrWhiteSpace(x.CustomerHomePhone))
			.WithMessage("Home phone or cell phone is required");
		RuleFor(x => x.CustomerEmail).EmailAddress();
		RuleForEach(e => e.Services).SetValidator(new FieldEstimateServiceValidator());
		RuleFor(x => x.EstimatedCost).Must(x => x >= 0).WithMessage("A valid job cost is required");
	}
}

public class FieldEstimateServiceValidator : AbstractValidator<JobServiceItem>
{
	public FieldEstimateServiceValidator()
	{
		RuleFor(e => e.ServiceName).NotEmpty();
		RuleFor(e => e.Quantity).GreaterThan(0);
		RuleFor(e => e.Price).GreaterThan(0);
	}
}

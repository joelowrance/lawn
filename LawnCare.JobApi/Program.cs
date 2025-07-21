using MassTransit;
using MediatR;
using FluentValidation;
using LawnCare.Shared.MessageContracts;
using LawnCare.Shared.Pipelines;

var builder = WebApplication.CreateBuilder(args);

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
		await _publishEndpoint.Publish(request.Estimate);
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
	}
}

public class FieldEstimateServiceValidator : AbstractValidator<FieldEstimateService>
{
	public FieldEstimateServiceValidator()
	{
		RuleFor(e => e.ServiceName).NotEmpty();
		RuleFor(e => e.Quantity).GreaterThan(0);
		RuleFor(e => e.Price).GreaterThan(0);
	}
}

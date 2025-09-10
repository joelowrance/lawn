using FluentValidation;

using LawnCare.JobApi.Domain.Services;
using LawnCare.Shared.Endpoints;
using LawnCare.Shared.MessageContracts;

using MassTransit;

using MediatR;

namespace LawnCare.JobApi.UseCases;

public class AcceptEstimate : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/estimate", async (IMediator mediator, FieldEstimate estimate) =>
		{
			await mediator.Send(new AcceptEstimateCommand(estimate));
			// needs a better result than just a status code
			return Results.Created();
		});
	}
}

public record AcceptEstimateCommand(FieldEstimate Estimate) : IRequest;

public class AcceptEstimateCommandHandler : IRequestHandler<AcceptEstimateCommand>
{
	readonly ILogger<AcceptEstimateCommandHandler> _logger;
	readonly IPublishEndpoint _publishEndpoint;
	private readonly IJobApplicationService  _jobApplicationService;

	public AcceptEstimateCommandHandler(
		ILogger<AcceptEstimateCommandHandler> logger,
		IPublishEndpoint publishEndpoint,
		IJobApplicationService jobApplicationService)
	{
		_logger = logger;
		_publishEndpoint = publishEndpoint;
		_jobApplicationService = jobApplicationService;
	}

	public async Task Handle(AcceptEstimateCommand request, CancellationToken cancellationToken)
	{
		// we want to
		// check if the customer exisits
		// create the customer
		// check if the address exists?
		// create the address
		// crete the job
		// use the saga to send out a welcome email
		// send out a




		var estimateCreatedResponse = await _jobApplicationService.CreateJobFromFieldEstimateAsync(request.Estimate);
        var job = estimateCreatedResponse.Job;

		_logger.LogInformation("Created job {JobId}", job.Id);
		_logger.LogInformation("Queueing estimate for customer validation: @{Estimate}", request.Estimate);

		var estimate = new EstimateReceivedEvent(
			job.TenantId,
			job.Id,
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

        if (estimateCreatedResponse.IsNewCustomer)
        {
            await _publishEndpoint.Publish(new SendWelcomeEmailCommand(estimate.Customer), cancellationToken);

        }
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

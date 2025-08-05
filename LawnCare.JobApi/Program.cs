using MassTransit;
using MassTransit.Mediator;
using MediatR;
using FluentValidation;

using JobService.Infrastructure.Persistence;

using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.Repositories;
using LawnCare.JobApi.Domain.Services;
using LawnCare.JobApi.Infrastructure.Database;
using LawnCare.Shared;
using LawnCare.Shared.EntityFramework;
using LawnCare.Shared.MessageContracts;
using LawnCare.Shared.OpenTelemetry;
using LawnCare.Shared.Pipelines;
using LawnCare.Shared.ProjectSetup;

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
builder.Services.AddTransient<JobDomainService,JobDomainService>();
builder.Services.AddTransient<IJobRepository, JobRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<JobDbContext>(dbContextOptionsBuilder =>
{
	dbContextOptionsBuilder.UseNpgsql(
			builder.Configuration.GetConnectionString("job-connection"))
		.UseSnakeCaseNamingConvention();
});
builder.Services.AddMigration<JobDbContext>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/estimate", async (MediatR.IMediator mediator, FieldEstimate estimate) =>
{
	await mediator.Send(new ProcessEstimateCommand(estimate));
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
	readonly ILogger<ProcessEstimateCommandHandler> _logger;
	readonly IPublishEndpoint _publishEndpoint;
	private readonly IJobApplicationService  _jobApplicationService;

	public ProcessEstimateCommandHandler(
		ILogger<ProcessEstimateCommandHandler> logger, 
		IPublishEndpoint publishEndpoint, 
		IJobApplicationService jobApplicationService)
	{
		_logger = logger;
		_publishEndpoint = publishEndpoint;
		_jobApplicationService = jobApplicationService;
	}

	public async Task Handle(ProcessEstimateCommand request, CancellationToken cancellationToken)
	{
		var job = await _jobApplicationService.CreateJobFromFieldEstimateAsync(request.Estimate);
		
		_logger.LogInformation("Created job {JobId}", job.Id);
		
		_logger.LogInformation("Queueing estimate: @{Estimate}", request.Estimate);		
		
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

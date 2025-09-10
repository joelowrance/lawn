using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Domain.Entities;
using LawnCare.CoreApi.Domain.ValueObjects;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.Shared.Endpoints;
using LawnCare.Shared.MessageContracts;
using MassTransit;
using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LawnCare.CoreApi.UseCases;

public class SubmitEstimate : IEndpoint
{
	/// <param name="app"></param>
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/estimate", async (IMediator mediator, JobEstimate estimate) =>
		{
			var result = await mediator.Send(new SubmitEstimateCommand(estimate));
			return Results.Created($"/jobs/{result.JobId}", result);
		});
	}
}


public record SubmitEstimateCommand(JobEstimate Estimate) : IRequest<SubmitEstimateResult>;

public record SubmitEstimateResult(JobId JobId, string CustomerName, string PropertyAddress);

public record SubmitEstimateCommandHandler : IRequestHandler<SubmitEstimateCommand, SubmitEstimateResult>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly CoreDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
	public SubmitEstimateCommandHandler(IUnitOfWork unitOfWork, CoreDbContext dbContext, IPublishEndpoint publishEndpoint)
	{
		_unitOfWork = unitOfWork;
		_dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

	public async Task<SubmitEstimateResult> Handle(SubmitEstimateCommand request, CancellationToken cancellationToken)
	{
		// TODO normalize phones
		var customer = await _dbContext.Customers
			.Where(x => x.Email == new EmailAddress(request.Estimate.CustomerEmail) ||
			                                          x.CellPhone  == new PhoneNumber(request.Estimate.CustomerCellPhone))
			.FirstOrDefaultAsync(cancellationToken);

        bool isNewCustomer = customer == null;

		if (customer == null)
		{
			customer = new Customer(request.Estimate.CustomerFirstName, request.Estimate.CustomerLastName,
				new EmailAddress(request.Estimate.CustomerEmail),
				new PhoneNumber(request.Estimate.CustomerHomePhone),
				new PhoneNumber(request.Estimate.CustomerCellPhone));
			_dbContext.Customers.Add(customer);
		}

		// TODO normalize addresses
		var location = await _dbContext.Locations
			.Where(x => x.Street1 == request.Estimate.CustomerAddress1 &&
			            x.Postcode == new Postcode(request.Estimate.CustomerZip))
			.FirstOrDefaultAsync(cancellationToken);


		if (location == null)
		{
			location = new Location(request.Estimate.CustomerAddress1, request.Estimate.CustomerAddress2,request.Estimate.CustomerAddress3,
				request.Estimate.CustomerCity, request.Estimate.CustomerState, new Postcode(request.Estimate.CustomerZip), customer);
			_dbContext.Locations.Add(location);
		}

		var job = new Job(request.Estimate.ScheduledDate.ToUniversalTime(), JobPriority.Normal, new Money(request.Estimate.EstimatedCost), location.LocationId);

		// Add service items from the estimate
		foreach (var service in request.Estimate.Services)
		{
			job.AddService(service.ServiceName, 1, service.Notes, new Money(service.Cost));
		}

		// Add description as a note
		if (!string.IsNullOrEmpty(request.Estimate.Description))
		{
			job.AddNote(request.Estimate.Description);
		}

		_dbContext.Jobs.Add(job);

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		var customerName = $"{customer.FirstName} {customer.LastName}";
		var propertyAddress = $"{location.Street1}, {location.City}, {location.State} {location.Postcode.Value}";

        if (isNewCustomer)
        {
            await _publishEndpoint.Publish(
                new SendWelcomeEmailCommand(
                    new CustomerInfo(
                        customer.Email.Value,
                        customer.FirstName,
                        customer.LastName,
                        customer.HomePhone.Value,
                        customer.CellPhone.Value,
                        new Address(
                            location.Street1,
                            location.Street2 ?? "",
                            location.Street3 ?? "",
                            location.City,
                            location.State,
                            location.Postcode.Value)
                    )), cancellationToken);
        }



		return new SubmitEstimateResult(job.JobId, customerName, propertyAddress);

	}
}

/// <summary>
/// An "estimate" from the field is generally in agreement with the customer.  The
/// rep has provided a price for the services requested by the client, and the two
/// are in agreement on the cost.  A best-guess job schedule day may also be provided,
/// but this can be highly dependent on weather, and staff/equipment availability
/// </summary>
public class JobEstimate
{
	public string UserId { get; set; } = string.Empty;
	//public string TenantId { get; set; } = null!;
	public string CustomerFirstName { get; set; }= string.Empty;
	public string CustomerLastName { get; set; }= string.Empty;
	public string CustomerAddress1 { get; set; }= string.Empty;
	public string CustomerAddress2 { get; set; }= string.Empty;
	public string CustomerAddress3 { get; set; }= string.Empty;
	public string CustomerCity { get; set; }= string.Empty;
	public string CustomerState { get; set; }= string.Empty;
	public string CustomerZip { get; set; }= string.Empty;
	public string CustomerHomePhone { get; set; }= string.Empty;
	public string CustomerCellPhone { get; set; }= string.Empty;
	public string CustomerEmail { get; set; }= string.Empty;
	public DateTimeOffset ScheduledDate { get; set; }
	public decimal EstimatedCost { get; set; }
	public int EstimatedDuration { get; set; }
	public string Description { get; set; }= string.Empty;
	public List<JobServiceItem> Services { get; set; } = [];
}

public class JobServiceItem
{
	public string ServiceName { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Cost { get; set; }
	public int DurationMinutes { get; set; }
	public string Notes { get; set; } = string.Empty;
}

public record JobEstimateLineItem(string ServiceName, int Quantity, string Comment, decimal Price);

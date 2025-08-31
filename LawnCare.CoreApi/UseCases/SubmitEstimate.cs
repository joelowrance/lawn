using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Domain.Entities;
using LawnCare.CoreApi.Domain.ValueObjects;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.Shared.Endpoints;
using LawnCare.Shared.MessageContracts;

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
			await mediator.Send(new SubmitEstimateCommand(estimate));
			// needs a better result than just a status code
			return Results.Created();
		});
	}
}


public record SubmitEstimateCommand(JobEstimate Estimate) : IRequest;

 
public record SubmitEstimateCommandHandler : IRequestHandler<SubmitEstimateCommand>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly CoreDbContext _dbContext;

	public SubmitEstimateCommandHandler(IUnitOfWork unitOfWork, CoreDbContext dbContext)
	{
		_unitOfWork = unitOfWork;
		_dbContext = dbContext;
	}

	public async Task Handle(SubmitEstimateCommand request, CancellationToken cancellationToken)
	{
		// TODO normalize phones
		var customer = await _dbContext.Customers
			.Where(x => x.Email == new EmailAddress(request.Estimate.CustomerEmail) ||
			                                          x.CellPhone  == new PhoneNumber(request.Estimate.CustomerCellPhone))
			.FirstOrDefaultAsync(cancellationToken);

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
		_dbContext.Jobs.Add(job);
		
		await _unitOfWork.SaveChangesAsync(cancellationToken);
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

public record JobEstimateLineItem(string ServiceName, int Quantity, string Comment, decimal Price); 
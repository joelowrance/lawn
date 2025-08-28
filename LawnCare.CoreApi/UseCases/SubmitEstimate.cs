using LawnCare.Shared.Endpoints;
using LawnCare.Shared.MessageContracts;

using MediatR;

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
	public Task Handle(SubmitEstimateCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
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
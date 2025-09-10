namespace LawnCare.JobApi.Application.DTOs
{
    public record EstimateCreatedResponse(JobResponse Job, bool IsNewCustomer);


	public record JobResponse(
		Guid Id,
		Guid TenantId,
		Guid? CustomerId,
		string CustomerName,
		JobAddressResponse Address,
		string Status,
		string Priority,
		string Description,
		string? SpecialInstructions,
		double EstimatedHours,
		decimal EstimatedCost,
		string Currency,
		decimal? ActualCost,
		DateTimeOffset? RequestedDate,
		DateTimeOffset? ScheduledDate,
		DateTimeOffset? CompletedDate,
		Guid? AssignedTechnicianId,
		IEnumerable<JobServiceItemResponse> ServiceItems,
		IEnumerable<JobNoteResponse> Notes,
		DateTimeOffset CreatedAt,
		DateTimeOffset UpdatedAt
	);

	public record JobAddressResponse(
		string Street1,
		string Street2,
		string Street3,
		string City,
		string State,
		string ZipCode,
		string? FullAddress,
		decimal? Latitude,
		decimal? Longitude
	);

	public record JobServiceItemResponse(
		string ServiceName,
		int Quantity,
		string Comment,
		decimal Price
	);


	public record JobNoteResponse(
		Guid Id,
		string Author,
		string Content,
		DateTimeOffset CreatedAt
	);
}

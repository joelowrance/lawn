using System;

namespace LawnCare.JobApi.Application.DTOs
{
	// public record CreateJobRequest(
	// 	Guid TenantId,
	// 	Guid CustomerId,
	// 	string CustomerName,
	// 	CreateJobAddressRequest Address,
	// 	CreateJobServiceRequest Service,
	// 	string Description,
	// 	string? SpecialInstructions,
	// 	DateTime RequestedDate,
	// 	int EstimatedHours,
	// 	int EstimatedMinutes,
	// 	decimal EstimatedCost
	// );

	// public record CreateJobAddressRequest(
	// 	string Street,
	// 	string City,
	// 	string State,
	// 	string ZipCode,
	// 	string? ApartmentUnit,
	// 	decimal? Latitude,
	// 	decimal? Longitude
	// );
	
	// public record AddJobRequirementRequest(
	// 	Guid JobId,
	// 	string RequirementType,
	// 	string Description,
	// 	bool IsRequired = true
	// );
	//
	// public record UpdateJobRequirementRequest(
	// 	string? RequirementType = null,
	// 	string? Description = null,
	// 	bool? IsRequired = null,
	// 	bool? IsFulfilled = null
	// );
	// public record CreateJobServiceRequest(
	// 	string Category,
	// 	string ServiceName,
	// 	string Description
	// );    
	//
	// public record AddJobNoteRequest(
	// 	Guid JobId,
	// 	string Author,
	// 	string Content
	// );
	//
	// public record UpdateJobNoteRequest(
	// 	string? Author = null,
	// 	string? Content = null
	// );
	//

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





	
    // public record CreateJobRequest(
    //     Guid TenantId,
    //     Guid CustomerId,
    //     string CustomerName,
    //     CreateJobAddressRequest Address,
    //     CreateJobServiceRequest Service,
    //     string Description,
    //     string? SpecialInstructions,
    //     DateTime RequestedDate,
    //     int EstimatedHours,
    //     int EstimatedMinutes,
    //     decimal EstimatedCost,
    //     string Currency = "USD",
    //     string Priority = "Normal"
    // );

    // public record CreateJobAddressRequest(
    //     string Street,
    //     string City,
    //     string State,
    //     string ZipCode,
    //     string? ApartmentUnit = null,
    //     decimal? Latitude = null,
    //     decimal? Longitude = null
    // );
    //
    // public record CreateJobServiceRequest(
    //     string Category,
    //     string ServiceName,
    //     string Description
    // );

    // public record CreateJobRequirementRequest(
    //     string RequirementType,
    //     string Description,
    //     bool IsRequired = true
    // );


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
    
    // public record UpdateJobRequest(
	   //  string? CustomerName = null,
	   //  string? Description = null,
	   //  string? SpecialInstructions = null,
	   //  DateTime? RequestedDate = null,
	   //  DateTime? ScheduledDate = null,
	   //  string? Priority = null,
	   //  string? Status = null,
	   //  Guid? AssignedTechnicianId = null,
	   //  UpdateJobAddressRequest? Address = null,
	   //  UpdateJobServiceRequest? Service = null,
	   //  UpdateJobDurationAndCostRequest? DurationAndCost = null
    // );
    //
    // public record UpdateJobAddressRequest(
	   //  string? Street = null,
	   //  string? City = null,
	   //  string? State = null,
	   //  string? ZipCode = null,
	   //  string? ApartmentUnit = null,
	   //  decimal? Latitude = null,
	   //  decimal? Longitude = null
    // );
    //
    // public record UpdateJobServiceRequest(
	   //  string? Category = null,
	   //  string? ServiceName = null,
	   //  string? Description = null
    // );
    //
    // public record UpdateJobDurationAndCostRequest(
	   //  int? EstimatedHours = null,
	   //  int? EstimatedMinutes = null,
	   //  decimal? EstimatedCost = null,
	   //  string? Currency = null,
	   //  decimal? ActualCost = null
    // );
}

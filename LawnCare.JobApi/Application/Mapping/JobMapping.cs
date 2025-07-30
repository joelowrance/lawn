using System.Linq;
using LawnCare.JobApi.Application.DTOs;
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.ValueObjects;
using LawnCare.JobApi.Domain.Enums;

namespace LawnCare.JobApi.Application.Mapping
{
    // public static class JobMapping
    // {
    //     // public static JobResponse ToJobResponse(Job job)
    //     // {
    //     //     return new JobResponse(
    //     //         Id: job.Id,
    //     //         TenantId: job.TenantId.Value,
    //     //         CustomerId: job.CustomerId?.Value,
    //     //         CustomerName: job.CustomerName,
    //     //         Address: new JobAddressResponse(
    //     //             Street: job.ServiceAddress.Street,
    //     //             City: job.ServiceAddress.City,
    //     //             State: job.ServiceAddress.State,
    //     //             ZipCode: job.ServiceAddress.ZipCode,
    //     //             ApartmentUnit: job.ServiceAddress.ApartmentUnit,
    //     //             FullAddress: job.ServiceAddress.FullAddress,
    //     //             Latitude: job.ServiceAddress.Latitude,
    //     //             Longitude: job.ServiceAddress.Longitude
    //     //         ),
    //     //         Service: new JobServiceResponse(
    //     //             Category: job.ServiceType.Category,
    //     //             ServiceName: job.ServiceType.ServiceName,
    //     //             Description: job.ServiceType.Description
    //     //         ),
    //     //         Status: job.Status.ToString(),
    //     //         Priority: job.Priority.ToString(),
    //     //         Description: job.Description,
    //     //         SpecialInstructions: job.SpecialInstructions,
    //     //         EstimatedHours: job.EstimatedDuration.TotalHours,
    //     //         EstimatedCost: job.EstimatedCost.Amount,
    //     //         Currency: job.EstimatedCost.Currency,
    //     //         ActualCost: job.ActualCost?.Amount,
    //     //         RequestedDate: job.RequestedDate,
    //     //         ScheduledDate: job.ScheduledDate,
    //     //         CompletedDate: job.CompletedDate,
    //     //         AssignedTechnicianId: job.AssignedTechnicianId?.Value,
    //     //         Requirements: job.Requirements.Select(r => new JobRequirementResponse(
    //     //             RequirementType: r.RequirementType,
    //     //             Description: r.Description,
    //     //             IsRequired: r.IsRequired,
    //     //             IsFulfilled: r.IsFulfilled
    //     //         )),
    //     //         Notes: job.Notes.Select(n => new JobNoteResponse(
    //     //             Id: n.Id,
    //     //             Author: n.Author,
    //     //             Content: n.Content,
    //     //             CreatedAt: n.CreatedAt
    //     //         )),
    //     //         CreatedAt: job.CreatedAt,
    //     //         UpdatedAt: job.UpdatedAt
    //     //     );
    //     // }
    //
    //     // public static ServiceAddress ToServiceAddress(CreateJobAddressRequest address)
    //     // {
    //     //     return new ServiceAddress(
    //     //         street1: address.Street,
    //     //         street1: address.Street,
    //     //         street1: address.Street,
    //     //         city: address.City,
    //     //         state: address.State,
    //     //         zipCode: address.ZipCode,
    //     //         apartmentUnit: address.ApartmentUnit,
    //     //         latitude: address.Latitude,
    //     //         longitude: address.Longitude
    //     //     );
    //     // }
    //
    //     // public static ServiceType ToServiceType(CreateJobServiceRequest service)
    //     // {
    //     //     return new ServiceType(
    //     //         category: service.Category,
    //     //         serviceName: service.ServiceName,
    //     //         description: service.Description
    //     //     );
    //     // }
    //     //
    //     // public static EstimatedDuration ToEstimatedDuration(int hours, int minutes)
    //     // {
    //     //     return new EstimatedDuration(hours, minutes);
    //     // }
    //     //
    //     // public static Money ToMoney(decimal amount, string currency = "USD")
    //     // {
    //     //     return new Money(amount, currency);
    //     // }
    // }
}

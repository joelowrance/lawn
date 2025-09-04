using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Domain.Entities;
using LawnCare.CoreApi.Domain.ValueObjects;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.Shared.Endpoints;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LawnCare.CoreApi.UseCases;

public class UpdateJob : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/jobs/{id:guid}", async (IMediator mediator, Guid id, UpdateJobRequest request) =>
        {
            var result = await mediator.Send(new UpdateJobCommand
            {
                JobId = id,
                Status = request.Status,
                Priority = request.Priority,
                RequestedServiceDate = request.RequestedServiceDate,
                JobCost = request.JobCost,
                ServiceItems = request.ServiceItems,
                Reason = request.Reason
            });

            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            return result.Error switch
            {
                "Job not found" => Results.NotFound(),
                _ => Results.BadRequest(result.Error)
            };
        });
    }
}

public record UpdateJobRequest
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTimeOffset? RequestedServiceDate { get; set; }
    public decimal? JobCost { get; set; }
    public List<ServiceItemRequest>? ServiceItems { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public record ServiceItemRequest
{
    public string ServiceName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Comment { get; set; }
    public decimal? Price { get; set; }
}

public record UpdateJobCommand : IRequest<Result<ServiceRequestDto>>
{
    public Guid JobId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTimeOffset? RequestedServiceDate { get; set; }
    public decimal? JobCost { get; set; }
    public List<ServiceItemRequest>? ServiceItems { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, Result<ServiceRequestDto>>
{
    private readonly CoreDbContext _dbContext;
    private readonly IJobMappingService _mappingService;
    private readonly ILogger<UpdateJobCommandHandler> _logger;

    public UpdateJobCommandHandler(
        CoreDbContext dbContext,
        IJobMappingService mappingService,
        ILogger<UpdateJobCommandHandler> logger)
    {
        _dbContext = dbContext;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<Result<ServiceRequestDto>> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var jobId = JobId.From(request.JobId);
            var job = await _dbContext.Jobs
                .Include(j => j.ServiceItems)
                .Include(j => j.Notes)
                .FirstOrDefaultAsync(j => j.JobId == jobId, cancellationToken);

            if (job == null)
            {
                return Result<ServiceRequestDto>.Failure("Job not found");
            }

            // Parse enums
            JobStatus? newStatus = null;
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<JobStatus>(request.Status, out var status))
            {
                newStatus = status;
            }

            JobPriority? newPriority = null;
            if (!string.IsNullOrEmpty(request.Priority) && Enum.TryParse<JobPriority>(request.Priority, out var priority))
            {
                newPriority = priority;
            }

            // Convert service items
            List<JobLineItem>? newServiceItems = null;
            if (request.ServiceItems != null)
            {
                newServiceItems = new List<JobLineItem>();
                foreach (var serviceItem in request.ServiceItems)
                {
                    var price = serviceItem.Price.HasValue ? new Money(serviceItem.Price.Value) : null;
                    var jobLineItem = new JobLineItem(job.JobId, serviceItem.ServiceName, serviceItem.Quantity, serviceItem.Comment, price);
                    newServiceItems.Add(jobLineItem);
                }
            }

            // Convert job cost
            Money? newJobCost = null;
            if (request.JobCost.HasValue)
            {
                newJobCost = new Money(request.JobCost.Value);
            }

            // Update job using single method
            job.UpdateJobDetails(
                newStatus,
                newPriority,
                request.RequestedServiceDate,
                newJobCost,
                newServiceItems,
                request.Reason
            );

            await _dbContext.SaveChangesAsync(cancellationToken);

            var location = _dbContext
                .Locations
                .AsNoTrackingWithIdentityResolution()
                .First(l => l.LocationId == job.LocationId);

            // Map to DTO and return
            var dto = _mappingService.MapToServiceRequestDto(job, location);
            return Result<ServiceRequestDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId}: {Message}", request.JobId, ex.Message);
            return Result<ServiceRequestDto>.Failure($"An error occurred while updating the job: {ex.Message}");
        }
    }
}

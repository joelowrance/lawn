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
                Notes = request.Notes
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
    public List<string>? Notes { get; set; }
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
    public List<string>? Notes { get; set; }
}

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, Result<ServiceRequestDto>>
{
    private readonly CoreDbContext _dbContext;
    private readonly JobMappingService _mappingService;
    private readonly ILogger<UpdateJobCommandHandler> _logger;

    public UpdateJobCommandHandler(
        CoreDbContext dbContext,
        JobMappingService mappingService,
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

            // Update job properties
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<JobStatus>(request.Status, out var status))
            {
                job.UpdateStatus(status);
            }

            if (!string.IsNullOrEmpty(request.Priority) && Enum.TryParse<JobPriority>(request.Priority, out var priority))
            {
                job.UpdatePriority(priority);
            }

            if (request.RequestedServiceDate.HasValue)
            {
                job.UpdateRequestedServiceDate(request.RequestedServiceDate.Value);
            }

            if (request.JobCost.HasValue)
            {
                var newCost = new Money(request.JobCost.Value);
                job.UpdateJobCost(newCost);
            }

            // Update service items
            if (request.ServiceItems != null)
            {
                job.ClearServices();
                foreach (var serviceItem in request.ServiceItems)
                {
                    var price = serviceItem.Price.HasValue ? new Money(serviceItem.Price.Value) : null;
                    job.AddService(serviceItem.ServiceName, serviceItem.Quantity, serviceItem.Comment, price);
                }
            }

            // Update notes
            if (request.Notes != null)
            {
                job.ClearNotes();
                foreach (var note in request.Notes.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    job.AddNote(note);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            var dto = await _mappingService.MapToServiceRequestDtoAsync(job);
            return Result<ServiceRequestDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId}", request.JobId);
            return Result<ServiceRequestDto>.Failure("An error occurred while updating the job");
        }
    }
}

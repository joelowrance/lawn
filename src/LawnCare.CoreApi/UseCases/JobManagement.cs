using LawnCare.CoreApi.Domain.Entities;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.Shared.Endpoints;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LawnCare.CoreApi.UseCases;

public class JobManagement : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // New unified search endpoint
        app.MapGet("/jobs/search", async (IMediator mediator,
            Guid? id = null,
            string? status = null,
            DateTime? date = null,
            bool? upcoming = null) =>
        {
            var jobs = await mediator.Send(new SearchJobsQuery
            {
                JobId = id,
                Status = status,
                Date = date,
                Upcoming = upcoming
            });
            return Results.Ok(jobs);
        });

        // Keep individual endpoints for backward compatibility, but route to search
        app.MapGet("/jobs/upcoming", async (IMediator mediator) =>
        {
            var jobs = await mediator.Send(new SearchJobsQuery { Upcoming = true });
            return Results.Ok(jobs);
        });

        app.MapGet("/jobs/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var jobs = await mediator.Send(new SearchJobsQuery { JobId = id });
            var job = jobs.FirstOrDefault();
            if (job == null)
                return Results.NotFound();
            return Results.Ok(job);
        });

        app.MapGet("/jobs/date/{date:datetime}", async (IMediator mediator, DateTime date) =>
        {
            var jobs = await mediator.Send(new SearchJobsQuery { Date = date });
            return Results.Ok(jobs);
        });

        app.MapGet("/jobs/status/{status}", async (IMediator mediator, string status) =>
        {
            var jobs = await mediator.Send(new SearchJobsQuery { Status = status });
            return Results.Ok(jobs);
        });
    }
}

// Unified Search Query
public record SearchJobsQuery : IRequest<List<ServiceRequestDto>>
{
    public Guid? JobId { get; init; }
    public string? Status { get; init; }
    public DateTime? Date { get; init; }
    public bool? Upcoming { get; init; }
}

// Shared Mapping Service Interface
public interface IJobMappingService
{
    ServiceRequestDto MapToServiceRequestDto(Job job, Location? location = null);
}

// Shared Mapping Service
public class JobMappingService : IJobMappingService
{
    private readonly CoreDbContext _dbContext;

    public JobMappingService(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ServiceRequestDto MapToServiceRequestDto(Job job, Location? location = null)
    {
        return new ServiceRequestDto
        {
            Id = job.JobId.Value,
            CustomerName = GetCustomerName(location),
            PropertyAddress = GetPropertyAddress(location),
            ServiceType = job.ServiceItems.FirstOrDefault()?.ServiceName ?? "General Service",
            Description = job.Notes.FirstOrDefault()?.Note ?? "No description",
            ScheduledDate = job.RequestedServiceDate?.DateTime.ToUniversalTime() ?? DateTime.UtcNow,
            ScheduledTime = TimeSpan.FromHours(9), // Default time
            EstimatedDuration = TimeSpan.FromHours(1), // Default duration
            Status = job.Status.ToString(),
            Priority = job.Priority.ToString(),
            AssignedTechnician = "Unassigned", // TODO: Add technician assignment
            EstimatedCost = job.JobCost.Amount,
            Notes = string.Join("; ", job.Notes.Select(n => n.Note)),
            PropertySize = "TBD", // TODO: Add property size
            SpecialInstructions = string.Join("; ", job.ServiceItems.Select(s => s.Comment).Where(c => !string.IsNullOrEmpty(c))),
            ContactPhone = GetContactPhone(location),
            ContactEmail = GetContactEmail(location),
            CreatedDate = job.CreatedAt.DateTime.ToUniversalTime(),
            LastModified = job.UpdatedAt.DateTime.ToUniversalTime()
        };
    }

    private static string GetCustomerName(Location? location)
    {
        return location?.Owner != null
            ? $"{location.Owner.FirstName} {location.Owner.LastName}"
            : "Customer Name";
    }

    private static string GetPropertyAddress(Location? location)
    {
        return location != null
            ? $"{location.Street1}, {location.City}, {location.State} {location.Postcode.Value}"
            : "Property Address";
    }

    private static string GetContactPhone(Location? location)
    {
        // TODO: Get from Location.Owner when phone field is available
        return "N/A";
    }

    private static string GetContactEmail(Location? location)
    {
        // TODO: Get from Location.Owner when email field is available
        return "N/A";
    }

    public ServiceRequestDto MapToServiceRequestDtoAsync(Job job, Location location)
    {
        return MapToServiceRequestDto(job, location);
    }
}

// Unified Search Query Handler
public class SearchJobsQueryHandler : IRequestHandler<SearchJobsQuery, List<ServiceRequestDto>>
{
    private readonly CoreDbContext _dbContext;
    private readonly IJobMappingService _mappingService;
    private readonly ILogger<SearchJobsQueryHandler> _logger;

    public SearchJobsQueryHandler(CoreDbContext dbContext, IJobMappingService mappingService, ILogger<SearchJobsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<List<ServiceRequestDto>> Handle(SearchJobsQuery request, CancellationToken cancellationToken)
    {
        // Build the base query with includes
        var query = _dbContext.Jobs
            .Include(j => j.ServiceItems)
            .Include(j => j.Notes)
            .AsQueryable();

        // Apply filters based on request parameters
        if (request.JobId.HasValue)
        {
            var jobId = JobId.From(request.JobId.Value);
            query = query.Where(j => j.JobId == jobId);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<JobStatus>(request.Status, out var status))
            {
                query = query.Where(j => j.Status == status);
            }
            else
            {
                // Invalid status, return empty list
                return new List<ServiceRequestDto>();
            }
        }

        if (request.Date.HasValue)
        {
            var targetDate = request.Date.Value.Date;
            var startOfDay = new DateTimeOffset(targetDate, TimeSpan.Zero);
            var endOfDay = startOfDay.AddDays(1);

            query = query.Where(j => j.RequestedServiceDate.HasValue &&
                                  j.RequestedServiceDate >= startOfDay &&
                                  j.RequestedServiceDate < endOfDay);
        }

        if (request.Upcoming == true)
        {
            // Ensure we're using UTC for PostgreSQL compatibility
            var currentDate = DateTimeOffset.UtcNow.Date.ToUniversalTime();
            query = query.Where(j => j.RequestedServiceDate >= currentDate);
        }

        // Order by requested service date
        query = query.OrderBy(j => j.RequestedServiceDate);

        // Execute the query
        List<Job> jobs;
        try
        {
            jobs = await query.ToListAsync(cancellationToken);

            if (!jobs.Any())
            {
                return new List<ServiceRequestDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching jobs with parameters: jobId={JobId}, status={Status}, date={Date}, upcoming={Upcoming}",
                request.JobId, request.Status, request.Date, request.Upcoming);
            return new List<ServiceRequestDto>();
        }

        // Load locations efficiently to avoid N+1 queries
        var locationIds = jobs.Select(j => j.LocationId).Distinct().ToList();
        var locations = await _dbContext.Locations
            .Where(l => locationIds.Contains(l.LocationId))
            .Include(l => l.Owner)
            .ToListAsync(cancellationToken);

        // Create a lookup for efficient location retrieval
        var locationLookup = locations.ToDictionary(l => l.LocationId, l => l);

        // Map jobs to DTOs using the shared mapping service
        return jobs.Select(job =>
        {
            var location = locationLookup.GetValueOrDefault(job.LocationId);
            return _mappingService.MapToServiceRequestDto(job, location);
        }).ToList();
    }
}

// DTO for the Management UI
public class ServiceRequestDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PropertyAddress { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string AssignedTechnician { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string PropertySize { get; set; } = string.Empty;
    public string SpecialInstructions { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
}

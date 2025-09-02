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
        app.MapGet("/jobs/upcoming", async (IMediator mediator) =>
        {
            var jobs = await mediator.Send(new GetUpcomingJobsQuery());
            return Results.Ok(jobs);
        });

        app.MapGet("/jobs/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var job = await mediator.Send(new GetJobByIdQuery(id));
            if (job == null)
                return Results.NotFound();
            return Results.Ok(job);
        });

        app.MapGet("/jobs/date/{date:datetime}", async (IMediator mediator, DateTime date) =>
        {
            var jobs = await mediator.Send(new GetJobsByDateQuery(date));
            return Results.Ok(jobs);
        });

        app.MapGet("/jobs/status/{status}", async (IMediator mediator, string status) =>
        {
            var jobs = await mediator.Send(new GetJobsByStatusQuery(status));
            return Results.Ok(jobs);
        });
    }
}

// Queries
public record GetUpcomingJobsQuery : IRequest<List<ServiceRequestDto>>;
public record GetJobByIdQuery(Guid JobId) : IRequest<ServiceRequestDto?>;
public record GetJobsByDateQuery(DateTime Date) : IRequest<List<ServiceRequestDto>>;
public record GetJobsByStatusQuery(string Status) : IRequest<List<ServiceRequestDto>>;

// Query Handlers
public class GetUpcomingJobsQueryHandler : IRequestHandler<GetUpcomingJobsQuery, List<ServiceRequestDto>>
{
    private readonly CoreDbContext _dbContext;

    public GetUpcomingJobsQueryHandler(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ServiceRequestDto>> Handle(GetUpcomingJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _dbContext.Jobs
            .Include(j => j.ServiceItems)
            .Include(j => j.Notes)
            .Where(j => j.RequestedServiceDate >= DateTimeOffset.UtcNow.Date)
            .OrderBy(j => j.RequestedServiceDate)
            .ToListAsync(cancellationToken);

        return jobs.Select(MapToServiceRequestDto).ToList();
    }

    private ServiceRequestDto MapToServiceRequestDto(Job job)
    {
        return new ServiceRequestDto
        {
            Id = job.JobId.Value,
            CustomerName = "Customer Name", // TODO: Get from Location.Owner
            PropertyAddress = "Property Address", // TODO: Get from Location
            ServiceType = job.ServiceItems.FirstOrDefault()?.ServiceName ?? "General Service",
            Description = job.Notes.FirstOrDefault()?.Note ?? "No description",
            ScheduledDate = job.RequestedServiceDate?.DateTime ?? DateTime.Now,
            ScheduledTime = TimeSpan.FromHours(9), // Default time
            EstimatedDuration = TimeSpan.FromHours(1), // Default duration
            Status = job.Status.ToString(),
            Priority = job.Priority.ToString(),
            AssignedTechnician = "Unassigned", // TODO: Add technician assignment
            EstimatedCost = job.JobCost.Amount,
            Notes = string.Join("; ", job.Notes.Select(n => n.Note)),
            PropertySize = "TBD", // TODO: Add property size
            SpecialInstructions = string.Join("; ", job.ServiceItems.Select(s => s.Comment).Where(c => !string.IsNullOrEmpty(c))),
            ContactPhone = "N/A", // TODO: Get from Location.Owner
            ContactEmail = "N/A", // TODO: Get from Location.Owner
            CreatedDate = job.CreatedAt.DateTime,
            LastModified = job.UpdatedAt.DateTime
        };
    }
}

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, ServiceRequestDto?>
{
    private readonly CoreDbContext _dbContext;

    public GetJobByIdQueryHandler(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceRequestDto?> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _dbContext.Jobs
            .Include(j => j.ServiceItems)
            .Include(j => j.Notes)
            .FirstOrDefaultAsync(j => j.JobId.Value == request.JobId, cancellationToken);

        if (job == null)
            return null;

        return new ServiceRequestDto
        {
            Id = job.JobId.Value,
            CustomerName = "Customer Name", // TODO: Get from Location.Owner
            PropertyAddress = "Property Address", // TODO: Get from Location
            ServiceType = job.ServiceItems.FirstOrDefault()?.ServiceName ?? "General Service",
            Description = job.Notes.FirstOrDefault()?.Note ?? "No description",
            ScheduledDate = job.RequestedServiceDate?.DateTime ?? DateTime.Now,
            ScheduledTime = TimeSpan.FromHours(9),
            EstimatedDuration = TimeSpan.FromHours(1),
            Status = job.Status.ToString(),
            Priority = job.Priority.ToString(),
            AssignedTechnician = "Unassigned",
            EstimatedCost = job.JobCost.Amount,
            Notes = string.Join("; ", job.Notes.Select(n => n.Note)),
            PropertySize = "TBD",
            SpecialInstructions = string.Join("; ", job.ServiceItems.Select(s => s.Comment).Where(c => !string.IsNullOrEmpty(c))),
            ContactPhone = "N/A",
            ContactEmail = "N/A",
            CreatedDate = job.CreatedAt.DateTime,
            LastModified = job.UpdatedAt.DateTime
        };
    }
}

public class GetJobsByDateQueryHandler : IRequestHandler<GetJobsByDateQuery, List<ServiceRequestDto>>
{
    private readonly CoreDbContext _dbContext;

    public GetJobsByDateQueryHandler(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ServiceRequestDto>> Handle(GetJobsByDateQuery request, CancellationToken cancellationToken)
    {
        var targetDate = request.Date.Date;
        var jobs = await _dbContext.Jobs
            .Include(j => j.ServiceItems)
            .Include(j => j.Notes)
            .Where(j => j.RequestedServiceDate.HasValue && 
                       j.RequestedServiceDate.Value.Date == targetDate)
            .OrderBy(j => j.RequestedServiceDate)
            .ToListAsync(cancellationToken);

        return jobs.Select(job => new ServiceRequestDto
        {
            Id = job.JobId.Value,
            CustomerName = "Customer Name",
            PropertyAddress = "Property Address",
            ServiceType = job.ServiceItems.FirstOrDefault()?.ServiceName ?? "General Service",
            Description = job.Notes.FirstOrDefault()?.Note ?? "No description",
            ScheduledDate = job.RequestedServiceDate?.DateTime ?? DateTime.Now,
            ScheduledTime = TimeSpan.FromHours(9),
            EstimatedDuration = TimeSpan.FromHours(1),
            Status = job.Status.ToString(),
            Priority = job.Priority.ToString(),
            AssignedTechnician = "Unassigned",
            EstimatedCost = job.JobCost.Amount,
            Notes = string.Join("; ", job.Notes.Select(n => n.Note)),
            PropertySize = "TBD",
            SpecialInstructions = string.Join("; ", job.ServiceItems.Select(s => s.Comment).Where(c => !string.IsNullOrEmpty(c))),
            ContactPhone = "N/A",
            ContactEmail = "N/A",
            CreatedDate = job.CreatedAt.DateTime,
            LastModified = job.UpdatedAt.DateTime
        }).ToList();
    }
}

public class GetJobsByStatusQueryHandler : IRequestHandler<GetJobsByStatusQuery, List<ServiceRequestDto>>
{
    private readonly CoreDbContext _dbContext;

    public GetJobsByStatusQueryHandler(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ServiceRequestDto>> Handle(GetJobsByStatusQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<JobStatus>(request.Status, out var status))
        {
            return new List<ServiceRequestDto>();
        }
        
        // well this got fucking messy quickly
        var jobs = await _dbContext.Jobs
            .Include(j => j.ServiceItems)
            .Include(j => j.Notes)
            .Where(j => j.Status == status)
            .OrderBy(j => j.RequestedServiceDate)
            .ToListAsync(cancellationToken);

        var locationIds = jobs.Select(x => x.LocationId);
        var locations = await _dbContext.Locations
	        .Where(x => locationIds.Contains(x.LocationId))
	        .Include(l => l.Owner)
	        .ToListAsync(cancellationToken);

        return jobs.Select(job =>
	        {
		        var location = locations.First(x => x.LocationId == job.LocationId);
		        return new ServiceRequestDto
		        {
			        Id = job.JobId.Value,
			        CustomerName = location.Owner.FirstName + " " + location.Owner.LastName,
			        PropertyAddress =
				        location.Street1 + ", " + location.City + ", " + location.State + " " + location.Postcode.Value,
			        ServiceType = job.ServiceItems.FirstOrDefault()?.ServiceName ?? "General Service",
			        Description = job.Notes.FirstOrDefault()?.Note ?? "No description",
			        ScheduledDate = job.RequestedServiceDate?.DateTime ?? DateTime.Now,
			        ScheduledTime = TimeSpan.FromHours(9),
			        EstimatedDuration = TimeSpan.FromHours(1),
			        Status = job.Status.ToString(),
			        Priority = job.Priority.ToString(),
			        AssignedTechnician = "Unassigned",
			        EstimatedCost = job.JobCost.Amount,
			        Notes = string.Join("; ", job.Notes.Select(n => n.Note)),
			        PropertySize = "TBD",
			        SpecialInstructions =
				        string.Join("; ",
					        job.ServiceItems.Select(s => s.Comment).Where(c => !string.IsNullOrEmpty(c))),
			        ContactPhone = "N/A",
			        ContactEmail = "N/A",
			        CreatedDate = job.CreatedAt.DateTime,
			        LastModified = job.UpdatedAt.DateTime
		        };
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
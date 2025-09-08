using LawnCare.CoreApi.Domain.DTOs;
using LawnCare.CoreApi.Domain.Entities;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.Shared.Endpoints;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LawnCare.CoreApi.UseCases;

public class TechnicianManagement : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // New unified search endpoint
        app.MapGet("/technicians/search", async (IMediator mediator,
            Guid? id = null,
            string? status = null,
            string? specialization = null,
            bool? active = null) =>
        {
            var technicians = await mediator.Send(new SearchTechniciansQuery
            {
                TechnicianId = id,
                Status = status,
                Specialization = specialization,
                Active = active
            });
            return Results.Ok(technicians);
        });

        // Keep individual endpoints for backward compatibility, but route to search
        app.MapGet("/technicians", async (IMediator mediator) =>
        {
            var technicians = await mediator.Send(new SearchTechniciansQuery());
            return Results.Ok(technicians);
        });

        app.MapGet("/technicians/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var technicians = await mediator.Send(new SearchTechniciansQuery { TechnicianId = id });
            var technician = technicians.FirstOrDefault();
            if (technician == null)
                return Results.NotFound();
            return Results.Ok(technician);
        });

        app.MapGet("/technicians/status/{status}", async (IMediator mediator, string status) =>
        {
            var technicians = await mediator.Send(new SearchTechniciansQuery { Status = status });
            return Results.Ok(technicians);
        });

        app.MapGet("/technicians/specialization/{specialization}", async (IMediator mediator, string specialization) =>
        {
            var technicians = await mediator.Send(new SearchTechniciansQuery { Specialization = specialization });
            return Results.Ok(technicians);
        });

        app.MapGet("/technicians/active", async (IMediator mediator) =>
        {
            var technicians = await mediator.Send(new SearchTechniciansQuery { Active = true });
            return Results.Ok(technicians);
        });
    }
}

// Unified Search Query
public record SearchTechniciansQuery : IRequest<List<TechnicianDto>>
{
    public Guid? TechnicianId { get; init; }
    public string? Status { get; init; }
    public string? Specialization { get; init; }
    public bool? Active { get; init; }
}

// Shared Mapping Service Interface
public interface ITechnicianMappingService
{
    TechnicianDto MapToTechnicianDto(Technician technician);
}

// Shared Mapping Service
public class TechnicianMappingService : ITechnicianMappingService
{
    public TechnicianDto MapToTechnicianDto(Technician technician)
    {
        return new TechnicianDto
        {
            Id = technician.Id,
            FirstName = technician.FirstName,
            LastName = technician.LastName,
            Email = technician.Email.Value,
            CellPhone = technician.CellPhone.Value,
            Address = technician.Address,
            PhotoUrl = technician.PhotoUrl,
            StartDate = technician.StartDate,
            Status = (int)technician.Status,
            StatusDisplay = technician.Status.ToString(),
            Specialization = (int)technician.Specialization,
            SpecializationDisplay = technician.Specialization.ToString(),
            HireDate = technician.HireDate,
            EmergencyContact = technician.EmergencyContact,
            EmergencyPhone = technician.EmergencyPhone.Value,
            LicenseNumber = technician.LicenseNumber,
            LicenseExpiry = technician.LicenseExpiry,
            Notes = technician.Notes,
            FullName = technician.GetFullName(),
            ExperienceDisplay = technician.GetExperienceDisplay(),
            YearsWithCompany = technician.GetYearsWithCompany(),
            MonthsWithCompany = technician.GetMonthsWithCompany(),
            CreatedAt = technician.CreatedAt,
            UpdatedAt = technician.UpdatedAt
        };
    }
}

// Unified Search Query Handler
public class SearchTechniciansQueryHandler : IRequestHandler<SearchTechniciansQuery, List<TechnicianDto>>
{
    private readonly CoreDbContext _dbContext;
    private readonly ITechnicianMappingService _mappingService;
    private readonly ILogger<SearchTechniciansQueryHandler> _logger;

    public SearchTechniciansQueryHandler(
        CoreDbContext dbContext, 
        ITechnicianMappingService mappingService, 
        ILogger<SearchTechniciansQueryHandler> logger)
    {
        _dbContext = dbContext;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<List<TechnicianDto>> Handle(SearchTechniciansQuery request, CancellationToken cancellationToken)
    {
        // Build the base query
        var query = _dbContext.Technicians.AsQueryable();

        // Apply filters based on request parameters
        if (request.TechnicianId.HasValue)
        {
            query = query.Where(t => t.Id == request.TechnicianId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<TechnicianStatus>(request.Status, out var status))
            {
                query = query.Where(t => t.Status == status);
            }
            else
            {
                // Invalid status, return empty list
                return new List<TechnicianDto>();
            }
        }

        if (!string.IsNullOrEmpty(request.Specialization))
        {
            if (Enum.TryParse<TechnicianSpecialization>(request.Specialization, out var specialization))
            {
                query = query.Where(t => t.Specialization == specialization);
            }
            else
            {
                // Invalid specialization, return empty list
                return new List<TechnicianDto>();
            }
        }

        if (request.Active == true)
        {
            query = query.Where(t => t.Status == TechnicianStatus.Active);
        }

        // Order by last name, then first name
        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        // Execute the query
        List<Technician> technicians;
        try
        {
            technicians = await query.ToListAsync(cancellationToken);

            if (!technicians.Any())
            {
                return new List<TechnicianDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching technicians with parameters: technicianId={TechnicianId}, status={Status}, specialization={Specialization}, active={Active}",
                request.TechnicianId, request.Status, request.Specialization, request.Active);
            return new List<TechnicianDto>();
        }

        // Map technicians to DTOs using the shared mapping service
        return technicians.Select(technician => _mappingService.MapToTechnicianDto(technician)).ToList();
    }
}

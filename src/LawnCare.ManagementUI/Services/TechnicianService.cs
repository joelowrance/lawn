using LawnCare.ManagementUI.Models;
using System.Text.Json;

namespace LawnCare.ManagementUI.Services;

public class TechnicianService : ITechnicianService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TechnicianService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TechnicianService(HttpClient httpClient, ILogger<TechnicianService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<Technician>> GetAllTechniciansAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all technicians from CoreAPI");
            var response = await _httpClient.GetAsync("/technicians");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var technicians = JsonSerializer.Deserialize<List<Technician>>(json, _jsonOptions);

            _logger.LogInformation("Successfully fetched {Count} technicians from CoreAPI", technicians?.Count ?? 0);
            return technicians ?? new List<Technician>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching technicians from CoreAPI");
            return new List<Technician>();
        }
    }

    public async Task<Technician?> GetTechnicianByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching technician {TechnicianId} from CoreAPI", id);
            var response = await _httpClient.GetAsync($"/technicians/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Technician {TechnicianId} not found in CoreAPI", id);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var technician = JsonSerializer.Deserialize<Technician>(json, _jsonOptions);

            _logger.LogInformation("Successfully fetched technician {TechnicianId} from CoreAPI", id);
            return technician;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching technician {TechnicianId} from CoreAPI", id);
            return null;
        }
    }

    public async Task<List<Technician>> GetTechniciansByStatusAsync(string status)
    {
        var allTechnicians = await GetAllTechniciansAsync();
        return allTechnicians.Where(t => t.StatusDisplay == status).ToList();
    }

    public async Task<List<Technician>> SearchTechniciansAsync(string searchTerm)
    {
        var allTechnicians = await GetAllTechniciansAsync();
        var term = searchTerm.ToLowerInvariant();
        return allTechnicians.Where(t =>
            t.FirstName.ToLowerInvariant().Contains(term) ||
            t.LastName.ToLowerInvariant().Contains(term) ||
            t.FullName.ToLowerInvariant().Contains(term) ||
            t.SpecializationDisplay.ToLowerInvariant().Contains(term)
        ).ToList();
    }

    public Task<Technician> CreateTechnicianAsync(Technician technician)
    {
        throw new NotImplementedException("Create technician functionality not yet implemented in API");
    }

    public Task<Technician> UpdateTechnicianAsync(Technician technician)
    {
        throw new NotImplementedException("Update technician functionality not yet implemented in API");
    }

    public Task<bool> DeleteTechnicianAsync(Guid id)
    {
        throw new NotImplementedException("Delete technician functionality not yet implemented in API");
    }

}

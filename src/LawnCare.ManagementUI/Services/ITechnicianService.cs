using LawnCare.ManagementUI.Models;

namespace LawnCare.ManagementUI.Services;

public interface ITechnicianService
{
    Task<List<Technician>> GetAllTechniciansAsync();
    Task<Technician?> GetTechnicianByIdAsync(Guid id);
    Task<List<Technician>> GetTechniciansByStatusAsync(string status);
    Task<List<Technician>> SearchTechniciansAsync(string searchTerm);
    Task<Technician> CreateTechnicianAsync(Technician technician);
    Task<Technician> UpdateTechnicianAsync(Technician technician);
    Task<bool> DeleteTechnicianAsync(Guid id);
}

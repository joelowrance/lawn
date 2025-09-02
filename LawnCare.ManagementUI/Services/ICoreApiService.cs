using LawnCare.ManagementUI.Models;

namespace LawnCare.ManagementUI.Services;

public interface ICoreApiService
{
    Task<List<ServiceRequest>> GetUpcomingJobsAsync();
    Task<ServiceRequest?> GetJobByIdAsync(Guid id);
    Task<List<ServiceRequest>> GetJobsByDateAsync(DateTime date);
    Task<List<ServiceRequest>> GetJobsByStatusAsync(string status);
    Task<ServiceRequest> CreateJobEstimateAsync(JobEstimate jobEstimate);
}
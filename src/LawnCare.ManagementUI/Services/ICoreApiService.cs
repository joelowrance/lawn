using LawnCare.ManagementUI.Models;

namespace LawnCare.ManagementUI.Services;

public interface ICoreApiService
{
    Task<List<ServiceRequest>> GetUpcomingJobsAsync();
    Task<ServiceRequest?> GetJobByIdAsync(Guid id);
    Task<List<ServiceRequest>> GetJobsByDateAsync(DateTime date);
    Task<List<ServiceRequest>> GetJobsByStatusAsync(string status);
    Task<List<ServiceRequest>> SearchJobsAsync(
        Guid? jobId = null,
        string? status = null,
        DateTime? date = null,
        bool? upcoming = null);
    Task<ServiceRequest> CreateJobEstimateAsync(JobEstimate jobEstimate);
    Task<ServiceRequest> UpdateJobAsync(Guid jobId, UpdateJobRequest updateRequest);
}
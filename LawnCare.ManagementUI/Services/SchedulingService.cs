using LawnCare.ManagementUI.Models;

namespace LawnCare.ManagementUI.Services;

public interface ISchedulingService
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
}

public class SchedulingService : ISchedulingService
{
    private readonly ICoreApiService _coreApiService;
    private readonly ILogger<SchedulingService> _logger;

    public SchedulingService(ICoreApiService coreApiService, ILogger<SchedulingService> logger)
    {
        _coreApiService = coreApiService;
        _logger = logger;
    }

    public async Task<List<ServiceRequest>> GetUpcomingJobsAsync()
    {
        try
        {
            return await _coreApiService.GetUpcomingJobsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming jobs from CoreAPI");
            return new List<ServiceRequest>();
        }
    }

    public async Task<ServiceRequest?> GetJobByIdAsync(Guid id)
    {
        try
        {
            return await _coreApiService.GetJobByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job {JobId} from CoreAPI", id);
            return null;
        }
    }

    public async Task<List<ServiceRequest>> GetJobsByDateAsync(DateTime date)
    {
        try
        {
            return await _coreApiService.GetJobsByDateAsync(date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs for date {Date} from CoreAPI", date);
            return new List<ServiceRequest>();
        }
    }

    public async Task<List<ServiceRequest>> GetJobsByStatusAsync(string status)
    {
        try
        {
            return await _coreApiService.GetJobsByStatusAsync(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs with status {Status} from CoreAPI", status);
            return new List<ServiceRequest>();
        }
    }

    public async Task<List<ServiceRequest>> SearchJobsAsync(
        Guid? jobId = null,
        string? status = null,
        DateTime? date = null,
        bool? upcoming = null)
    {
        try
        {
            return await _coreApiService.SearchJobsAsync(jobId, status, date, upcoming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching jobs with parameters: jobId={JobId}, status={Status}, date={Date}, upcoming={Upcoming}", 
                jobId, status, date, upcoming);
            return new List<ServiceRequest>();
        }
    }

    public async Task<ServiceRequest> CreateJobEstimateAsync(JobEstimate jobEstimate)
    {
        try
        {
            return await _coreApiService.CreateJobEstimateAsync(jobEstimate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job estimate in CoreAPI");
            throw;
        }
    }
}
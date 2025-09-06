using System.Text;
using System.Text.Json;
using LawnCare.ManagementUI.Models;

namespace LawnCare.ManagementUI.Services;

public class CoreApiService : ICoreApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoreApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CoreApiService(HttpClient httpClient, ILogger<CoreApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<ServiceRequest>> GetUpcomingJobsAsync()
    {
        return await SearchJobsAsync(upcoming: true);
    }

    public async Task<ServiceRequest?> GetJobByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching job {JobId} from CoreAPI", id);
            var jobs = await SearchJobsAsync(jobId: id);
            
            var job = jobs.FirstOrDefault();
            if (job == null)
            {
                _logger.LogWarning("Job {JobId} not found in CoreAPI", id);
                return null;
            }
            
            _logger.LogInformation("Deserialized job {JobId}: {JobDetails}", id, job.CustomerName);
            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching job {JobId} from CoreAPI", id);
            return null;
        }
    }

    public async Task<List<ServiceRequest>> GetJobsByDateAsync(DateTime date)
    {
        return await SearchJobsAsync(date: date);
    }

    public async Task<List<ServiceRequest>> GetJobsByStatusAsync(string status)
    {
        return await SearchJobsAsync(status: status);
    }

    // New unified search method
    public async Task<List<ServiceRequest>> SearchJobsAsync(
        Guid? jobId = null,
        string? status = null,
        DateTime? date = null,
        bool? upcoming = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (jobId.HasValue)
                queryParams.Add($"id={jobId.Value}");
            
            if (!string.IsNullOrEmpty(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");
            
            if (date.HasValue)
                queryParams.Add($"date={date.Value:yyyy-MM-dd}");
            
            if (upcoming.HasValue)
                queryParams.Add($"upcoming={upcoming.Value.ToString().ToLower()}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/jobs/search{queryString}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<ServiceRequest>>(json, _jsonOptions);
            return jobs ?? new List<ServiceRequest>();
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
            var json = JsonSerializer.Serialize(jobEstimate, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/estimate", content);
            response.EnsureSuccessStatusCode();
            
            // The CoreAPI should return the created job/service request
            var responseJson = await response.Content.ReadAsStringAsync();
            var createdJob = JsonSerializer.Deserialize<ServiceRequest>(responseJson, _jsonOptions);
            
            if (createdJob == null)
            {
                throw new InvalidOperationException("CoreAPI returned null job after creation");
            }
            
            return createdJob;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job estimate in CoreAPI");
            throw;
        }
    }

    public async Task<ServiceRequest> UpdateJobAsync(Guid jobId, UpdateJobRequest updateRequest)
    {
        try
        {
            var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/jobs/{jobId}", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedJob = JsonSerializer.Deserialize<ServiceRequest>(responseJson, _jsonOptions);
            
            if (updatedJob == null)
            {
                throw new InvalidOperationException("CoreAPI returned null job after update");
            }
            
            return updatedJob;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId} in CoreAPI", jobId);
            throw;
        }
    }
}
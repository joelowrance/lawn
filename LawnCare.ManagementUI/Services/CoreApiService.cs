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
        try
        {
            var response = await _httpClient.GetAsync("/jobs/upcoming");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<ServiceRequest>>(json, _jsonOptions);
            return jobs ?? new List<ServiceRequest>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching upcoming jobs from CoreAPI");
            return new List<ServiceRequest>();
        }
    }

    public async Task<ServiceRequest?> GetJobByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching job {JobId} from CoreAPI", id);
            var response = await _httpClient.GetAsync($"/jobs/{id}");
            
            _logger.LogInformation("Response status: {StatusCode} for job {JobId}", response.StatusCode, id);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Job {JobId} not found in CoreAPI", id);
                return null;
            }
                
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received JSON response for job {JobId}: {Json}", id, json);
            
            var job = JsonSerializer.Deserialize<ServiceRequest>(json, _jsonOptions);
            _logger.LogInformation("Deserialized job {JobId}: {JobDetails}", id, job?.CustomerName);
            
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
        try
        {
            var dateString = date.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"/jobs/date/{dateString}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<ServiceRequest>>(json, _jsonOptions);
            return jobs ?? new List<ServiceRequest>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching jobs for date {Date} from CoreAPI", date);
            return new List<ServiceRequest>();
        }
    }

    public async Task<List<ServiceRequest>> GetJobsByStatusAsync(string status)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/jobs/status/{Uri.EscapeDataString(status)}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<ServiceRequest>>(json, _jsonOptions);
            return jobs ?? new List<ServiceRequest>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching jobs with status {Status} from CoreAPI", status);
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
}
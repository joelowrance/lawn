using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.Enums;
using LawnCare.JobApi.Domain.ValueObjects;

namespace LawnCare.JobApi.Domain.Repositories;

public interface IJobRepository
{
	Task<Job?> GetByIdAsync(JobId jobId, TenantId tenantId);
	Task<List<Job>> GetByTenantAsync(TenantId tenantId, JobStatus? status = null);
	Task<List<Job>> GetByCustomerAsync(TenantId tenantId, CustomerId customerId);
	Task<List<Job>> GetScheduledJobsAsync(TenantId tenantId, DateTime startDate, DateTime endDate);
	Task AddAsync(Job job);
	Task UpdateAsync(Job job);
	Task DeleteAsync(Job job);
}
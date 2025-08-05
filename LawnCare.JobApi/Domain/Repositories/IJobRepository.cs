using JobService.Infrastructure.Persistence;

using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.Enums;
using LawnCare.JobApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace LawnCare.JobApi.Domain.Repositories;

public interface IJobRepository
{
	Task<Job?> GetByIdAsync(JobId jobId, TenantId tenantId);
	Task<List<Job>> GetByTenantAsync(TenantId tenantId, JobStatus? status = null);
	Task<List<Job>> GetByCustomerAsync(TenantId tenantId, CustomerId customerId);
	Task<List<Job>> GetScheduledJobsAsync(TenantId tenantId, DateTime startDate, DateTime endDate);
	Task AddAsync(Job job);
	// Task UpdateAsync(Job job);
	// Task DeleteAsync(Job job);
}

public class JobRepository : IJobRepository
{
	JobDbContext _dbContext;

	public JobRepository(JobDbContext dbContext)
	{
		_dbContext = dbContext;
	}


	public async Task<Job?> GetByIdAsync(JobId jobId, TenantId tenantId)
	{
		return await _dbContext.Jobs.FirstOrDefaultAsync(x => x.JobId == jobId && x.TenantId == tenantId);
		
	}

	public async Task<List<Job>> GetByTenantAsync(TenantId tenantId, JobStatus? status = null)
	{
		var query = _dbContext.Jobs.Where(x => x.TenantId == tenantId);
		if (status.HasValue)
		{
			query = query.Where(x => x.Status == status.Value);
		}
		
		return await query.ToListAsync();
	}

	public async Task<List<Job>> GetByCustomerAsync(TenantId tenantId, CustomerId customerId)
	{
		return await (_dbContext.Jobs.Where(x => x.TenantId == tenantId && x.CustomerId == customerId))
			.ToListAsync();
	}

	public async Task<List<Job>> GetScheduledJobsAsync(TenantId tenantId, DateTime startDate, DateTime endDate)
	{
		return await (_dbContext.Jobs.Where(x => x.TenantId == tenantId && x.ScheduledDate >= startDate && x.ScheduledDate <= endDate))
			.ToListAsync();
	}

	public async Task AddAsync(Job job)
	{
		await _dbContext.Jobs.AddAsync(job);
	}

	//public Task UpdateAsync(Job job)
	//{
	//	
	//}

	// public Task DeleteAsync(Job job)
	// {
	// 	throw new NotImplementedException();
	// }
}

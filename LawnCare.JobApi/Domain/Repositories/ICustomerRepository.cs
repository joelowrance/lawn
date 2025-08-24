using JobService.Infrastructure.Persistence;

using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.ValueObjects;
using LawnCare.Shared.MessageContracts;

using Microsoft.EntityFrameworkCore;

namespace LawnCare.JobApi.Domain.Repositories;

public interface ICustomerRepository
{
	Task<Customer?> FindByFieldEstimateAttributes(FieldEstimate estimate);
	Task<Customer?> GetByIdAsync(CustomerId jobId);
	Task<Customer> CreateCustomerAsync();
	Task AddAsync(Customer customer);
}
	
public class CustomerRepository : ICustomerRepository
{
	private readonly JobDbContext _dbContext;

	public CustomerRepository(JobDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<Customer?> FindByFieldEstimateAttributes(FieldEstimate estimate)
	{
		var existing = await _dbContext.Customers.FirstOrDefaultAsync(x => 
			x.Email == estimate.CustomerEmail 
			|| x.CellPhone == estimate.CustomerCellPhone 
			|| x.Address.Street1.Equals(estimate.CustomerAddress1, StringComparison.InvariantCultureIgnoreCase));
		
		return existing;
	}

	public async Task<Customer?> GetByIdAsync(CustomerId customerId)
	{
		return await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
	}

	public Task<Customer> CreateCustomerAsync()
	{
		throw new NotImplementedException();
	}

	public async Task AddAsync(Customer customer)
	{
		await _dbContext.Customers.AddAsync(customer);
	}
}
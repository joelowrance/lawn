using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace LawnCare.CustomerApi.Infrastructure
{
	public interface ICustomerService
	{
		// Task<Customer?> GetByIdAsync(Guid tenantId, Guid customerId);
		// Task<IEnumerable<Customer>> GetAllAsync(Guid tenantId, int page = 1, int pageSize = 50);
		// Task<IEnumerable<Customer>> SearchAsync(Guid tenantId, string searchTerm, int page = 1, int pageSize = 50);
		// Task<Customer> CreateAsync(Guid tenantId, Customer createDto, string createdBy);
		// Task<Customer?> UpdateAsync(Guid tenantId, Guid customerId, Customer updateDto, string updatedBy);
		// Task<bool> DeleteAsync(Guid tenantId, Guid customerId, string deletedBy);
		// Task<bool> ChangeStatusAsync(Guid tenantId, Guid customerId, CustomerStatus newStatus, string changedBy);
	}

	public class CustomerService : ICustomerService
	{
		private readonly CustomerDbContext _context;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly ILogger<CustomerService> _logger;

		public CustomerService(
			CustomerDbContext context,
			IPublishEndpoint publishEndpoint,
			ILogger<CustomerService> logger)
		{
			_context = context;
			_publishEndpoint = publishEndpoint;
			_logger = logger;
		}

		// public async Task<CustomerDto?> GetByIdAsync(Guid tenantId, Guid customerId)
		// {
		//     _logger.LogDebug("Getting customer {CustomerId} for tenant {TenantId}", customerId, tenantId);
		//     
		//     var customer = await _context.Customers
		//         .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId);
		//         
		//     if (customer == null)
		//     {
		//         _logger.LogWarning("Customer {CustomerId} not found for tenant {TenantId}", customerId, tenantId);
		//         return null;
		//     }
		//     
		//     return MapToDto(customer);
		// }
		//
		// public async Task<IEnumerable<CustomerDto>> GetAllAsync(Guid tenantId, int page = 1, int pageSize = 50)
		// {
		//     _logger.LogDebug("Getting customers for tenant {TenantId}, page {Page}, pageSize {PageSize}", 
		//         tenantId, page, pageSize);
		//     
		//     var customers = await _context.Customers
		//         .Where(c => c.TenantId == tenantId)
		//         .OrderBy(c => c.LastName)
		//         .ThenBy(c => c.FirstName)
		//         .Skip((page - 1) * pageSize)
		//         .Take(pageSize)
		//         .ToListAsync();
		//         
		//     _logger.LogDebug("Found {CustomerCount} customers for tenant {TenantId}", customers.Count, tenantId);
		//     
		//     return customers.Select(MapToDto);
		// }
		//
		// public async Task<IEnumerable<CustomerDto>> SearchAsync(Guid tenantId, string searchTerm, int page = 1, int pageSize = 50)
		// {
		//     _logger.LogDebug("Searching customers for tenant {TenantId} with term '{SearchTerm}'", tenantId, searchTerm);
		//     
		//     var customers = await _context.Customers
		//         .Where(c => c.TenantId == tenantId &&
		//             (c.FirstName.Contains(searchTerm) ||
		//              c.LastName.Contains(searchTerm) ||
		//              c.Email.Contains(searchTerm) ||
		//              (c.Phone != null && c.Phone.Contains(searchTerm))))
		//         .OrderBy(c => c.LastName)
		//         .ThenBy(c => c.FirstName)
		//         .Skip((page - 1) * pageSize)
		//         .Take(pageSize)
		//         .ToListAsync();
		//         
		//     _logger.LogDebug("Found {CustomerCount} customers matching search term", customers.Count);
		//     
		//     return customers.Select(MapToDto);
		// }
		//
		// public async Task<CustomerDto> CreateAsync(Guid tenantId, CreateCustomerDto createDto, string createdBy)
		// {
		//     _logger.LogInformation("Creating customer for tenant {TenantId}", tenantId);
		//     
		//     var customer = new Customer
		//     {
		//         Id = Guid.NewGuid(),
		//         TenantId = tenantId,
		//         FirstName = createDto.FirstName,
		//         LastName = createDto.LastName,
		//         Email = createDto.Email,
		//         Phone = createDto.Phone,
		//         Address = createDto.Address,
		//         City = createDto.City,
		//         State = createDto.State,
		//         ZipCode = createDto.ZipCode,
		//         CustomerType = createDto.CustomerType,
		//         Status = CustomerStatus.Active,
		//         Notes = createDto.Notes,
		//         CreatedBy = createdBy,
		//         UpdatedBy = createdBy
		//     };
		//     
		//     _context.Customers.Add(customer);
		//     await _context.SaveChangesAsync();
		//     
		//     _logger.LogInformation("Customer {CustomerId} created successfully", customer.Id);
		//     
		//     // Publish event
		//     await _publishEndpoint.Publish(new CustomerCreated
		//     {
		//         CustomerId = customer.Id,
		//         TenantId = customer.TenantId,
		//         FirstName = customer.FirstName,
		//         LastName = customer.LastName,
		//         Email = customer.Email,
		//         Phone = customer.Phone,
		//         Address = customer.Address,
		//         City = customer.City,
		//         State = customer.State,
		//         ZipCode = customer.ZipCode,
		//         Timestamp = DateTime.UtcNow,
		//         CreatedBy = createdBy
		//     });
		//     
		//     return MapToDto(customer);
		// }
		//
		// public async Task<CustomerDto?> UpdateAsync(Guid tenantId, Guid customerId, UpdateCustomerDto updateDto, string updatedBy)
		// {
		//     _logger.LogInformation("Updating customer {CustomerId} for tenant {TenantId}", customerId, tenantId);
		//     
		//     var customer = await _context.Customers
		//         .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId);
		//         
		//     if (customer == null)
		//     {
		//         _logger.LogWarning("Customer {CustomerId} not found for update", customerId);
		//         return null;
		//     }
		//     
		//     var oldStatus = customer.Status;
		//     
		//     customer.FirstName = updateDto.FirstName;
		//     customer.LastName = updateDto.LastName;
		//     customer.Email = updateDto.Email;
		//     customer.Phone = updateDto.Phone;
		//     customer.Address = updateDto.Address;
		//     customer.City = updateDto.City;
		//     customer.State = updateDto.State;
		//     customer.ZipCode = updateDto.ZipCode;
		//     customer.CustomerType = updateDto.CustomerType;
		//     customer.Status = updateDto.Status;
		//     customer.Notes = updateDto.Notes;
		//     customer.UpdatedBy = updatedBy;
		//     
		//     await _context.SaveChangesAsync();
		//     
		//     _logger.LogInformation("Customer {CustomerId} updated successfully", customerId);
		//     
		//     // Publish events
		//     await _publishEndpoint.Publish(new CustomerUpdated
		//     {
		//         CustomerId = customer.Id,
		//         TenantId = customer.TenantId,
		//         FirstName = customer.FirstName,
		//         LastName = customer.LastName,
		//         Email = customer.Email,
		//         Phone = customer.Phone,
		//         Address = customer.Address,
		//         City = customer.City,
		//         State = customer.State,
		//         ZipCode = customer.ZipCode,
		//         Timestamp = DateTime.UtcNow,
		//         UpdatedBy = updatedBy
		//     });
		//     
		//     // Publish status change event if status changed
		//     if (oldStatus != customer.Status)
		//     {
		//         await _publishEndpoint.Publish(new CustomerStatusChanged
		//         {
		//             CustomerId = customer.Id,
		//             TenantId = customer.TenantId,
		//             OldStatus = oldStatus,
		//             NewStatus = customer.Status,
		//             Timestamp = DateTime.UtcNow,
		//             ChangedBy = updatedBy
		//         });
		//     }
		//     
		//     return MapToDto(customer);
		// }
		//
		// public async Task<bool> DeleteAsync(Guid tenantId, Guid customerId, string deletedBy)
		// {
		//     _logger.LogInformation("Deleting customer {CustomerId} for tenant {TenantId}", customerId, tenantId);
		//     
		//     var customer = await _context.Customers
		//         .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId);
		//         
		//     if (customer == null)
		//     {
		//         _logger.LogWarning("Customer {CustomerId} not found for deletion", customerId);
		//         return false;
		//     }
		//     
		//     _context.Customers.Remove(customer);
		//     await _context.SaveChangesAsync();
		//     
		//     _logger.LogInformation("Customer {CustomerId} deleted successfully", customerId);
		//     
		//     // Publish event
		//     await _publishEndpoint.Publish(new CustomerDeleted
		//     {
		//         CustomerId = customerId,
		//         TenantId = tenantId,
		//         Timestamp = DateTime.UtcNow,
		//         DeletedBy = deletedBy
		//     });
		//     
		//     return true;
		// }
		//
		// public async Task<bool> ChangeStatusAsync(Guid tenantId, Guid customerId, CustomerStatus newStatus, string changedBy)
		// {
		//     _logger.LogInformation("Changing status for customer {CustomerId} to {NewStatus}", customerId, newStatus);
		//     
		//     var customer = await _context.Customers
		//         .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId);
		//         
		//     if (customer == null)
		//     {
		//         _logger.LogWarning("Customer {CustomerId} not found for status change", customerId);
		//         return false;
		//     }
		//     
		//     var oldStatus = customer.Status;
		//     customer.Status = newStatus;
		//     customer.UpdatedBy = changedBy;
		//     
		//     await _context.SaveChangesAsync();
		//     
		//     _logger.LogInformation("Customer {CustomerId} status changed from {OldStatus} to {NewStatus}", 
		//         customerId, oldStatus, newStatus);
		//     
		//     // Publish event
		//     await _publishEndpoint.Publish(new CustomerStatusChanged
		//     {
		//         CustomerId = customerId,
		//         TenantId = tenantId,
		//         OldStatus = oldStatus,
		//         NewStatus = newStatus,
		//         Timestamp = DateTime.UtcNow,
		//         ChangedBy = changedBy
		//     });
		//     
		//     return true;
		// }
		//
		// private static CustomerDto MapToDto(Customer customer)
		// {
		//     return new CustomerDto
		//     {
		//         Id = customer.Id,
		//         TenantId = customer.TenantId,
		//         FirstName = customer.FirstName,
		//         LastName = customer.LastName,
		//         Email = customer.Email,
		//         Phone = customer.Phone,
		//         Address = customer.Address,
		//         City = customer.City,
		//         State = customer.State,
		//         ZipCode = customer.ZipCode,
		//         CustomerType = customer.CustomerType,
		//         Status = customer.Status,
		//         Notes = customer.Notes,
		//         CreatedAt = customer.CreatedAt,
		//         UpdatedAt = customer.UpdatedAt
		//     };
		// }
	}
}

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace LawnCare.CustomerApi.Infrastructure
{
	public interface ICustomerService
	{
		Task ProcessCustomerAsync(LawnCare.Shared.MessageContracts.ProcessCustomerCommand command);
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

		public async Task ProcessCustomerAsync(LawnCare.Shared.MessageContracts.ProcessCustomerCommand command)
		{
			try
			{
				_logger.LogInformation("Processing customer with email {Email} for tenant {TenantId}", 
					command.Customer.Email, command.TenantId);

				// Check if the customer exists
				var existingCustomer = await _context.Customers
					.FirstOrDefaultAsync(c => c.TenantId == command.TenantId && 
					                     c.Email == command.Customer.Email);

				if (existingCustomer != null)
				{
					_logger.LogInformation("Found existing customer with ID {CustomerId}", existingCustomer.Id);

					// Publish CustomerFoundEvent
					await _publishEndpoint.Publish(new LawnCare.Shared.MessageContracts.CustomerFoundEvent(
						command.TenantId,
						command.EstimateId,
						existingCustomer.Id,
						IsNewCustomer: false));
				}
				else
				{
					_logger.LogInformation("Customer not found, creating new customer");

					// Create the new customer
					var newCustomer = new Customer
					{
						Id = Guid.NewGuid(),
						TenantId = command.TenantId,
						FirstName = command.Customer.FirstName,
						LastName = command.Customer.LastName,
						Email = command.Customer.Email,
						HomePhone = command.Customer.HomePhone,
						CellPhone = command.Customer.CellPhone,
						Address1 = command.Customer.Address.Street,
						Address2 = command.Customer.Address.Street2,
						Address3 = command.Customer.Address.Street3,
						City = command.Customer.Address.City,
						State = command.Customer.Address.State,
						ZipCode = command.Customer.Address.ZipCode,
						CustomerType = CustomerType.Residential, // Default to residential
						Status = CustomerStatus.Active,
						CreatedBy = "System",
						UpdatedBy = "System"
					};

					_context.Customers.Add(newCustomer);
					await _context.SaveChangesAsync();

					_logger.LogInformation("Created new customer with ID {CustomerId}", newCustomer.Id);

					// Publish CustomerCreatedEvent
					await _publishEndpoint.Publish(new LawnCare.Shared.MessageContracts.CustomerCreatedEvent(
						command.TenantId,
						command.EstimateId,
						newCustomer.Id));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing customer: {ErrorMessage}", ex.Message);

				// Publish CustomerProcessingFailedEvent
				await _publishEndpoint.Publish(new LawnCare.Shared.MessageContracts.CustomerProcessingFailedEvent(
					command.TenantId,
					command.EstimateId,
					$"Failed to process customer: {ex.Message}"));

				// Rethrow to let MassTransit retry policy handle it
				throw;
			}
		}
		
	}
}

using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.ValueObjects;

namespace LawnCare.JobApi.Domain.Entities
{
	public class Customer : Entity
	{
		public CustomerId CustomerId { get; set; } = null!;
		public Guid TenantId { get; set; }

		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public string? HomePhone { get; set; }
		public string? CellPhone { get; set; }
		
		// todo: billing vs actual address
		public Address Address { get; set; } = null!;
		public Guid AddressId { get; set; }
		
		public CustomerType CustomerType { get; set; } = CustomerType.Residential;
		public CustomerStatus Status { get; set; } = CustomerStatus.Active;

		public string? Notes { get; set; }
        
		public DateTimeOffset? CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public string CreatedBy { get; set; } = string.Empty;
		public string UpdatedBy { get; set; } = string.Empty;
		
		private Customer() { }

		public Customer(string firstName
		, string lastName
			, string emailAddress
		, string cellPhone
		, string homePhone
		, Address billingAddress
		)
		{
			FirstName = firstName;
			LastName = lastName;
			Email = emailAddress;
			CellPhone = cellPhone;
			HomePhone = homePhone;
			CreatedAt = DateTime.UtcNow;
			Address = billingAddress;
		}
	}
	
	public enum CustomerType
	{
		Residential = 1,
		Commercial = 2
	}
    
	public enum CustomerStatus
	{
		Active = 1,
		Inactive = 2,
		Suspended = 3
	}
}
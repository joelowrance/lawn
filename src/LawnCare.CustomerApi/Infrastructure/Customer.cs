using System.ComponentModel.DataAnnotations;

namespace LawnCare.CustomerApi.Infrastructure;

public record CustomerInfo(string Email, string FirstName, string LastName, string HomePhone, string CellPhone, Address Address);
//public record JobDetails(DateTimeOffset ScheduledDate, decimal EstimatedCost, string Description, JobServiceItem[] ServicesRequested);
public record Address(string Street, string Street2, string Street3, string City, string State, string ZipCode);


public class Customer
{
	public Guid Id { get; set; }
	public Guid TenantId { get; set; }
        
	[Required]
	[MaxLength(100)]
	public string FirstName { get; set; } = string.Empty;
        
	[Required]
	[MaxLength(100)]
	public string LastName { get; set; } = string.Empty;
        
	[Required]
	[EmailAddress]
	[MaxLength(255)]
	public string Email { get; set; } = string.Empty;

	[MaxLength(20)]
	public string? HomePhone { get; set; }
	[MaxLength(20)]
	public string? CellPhone { get; set; }
        
        
	[Required]
	[MaxLength(255)]
	public string Address1 { get; set; } = string.Empty;
	
	[Required]
	[MaxLength(255)]
	public string Address2 { get; set; } = string.Empty;
	
	[Required]
	[MaxLength(255)]
	public string Address3 { get; set; } = string.Empty;
        
	[Required]
	[MaxLength(100)]
	public string City { get; set; } = string.Empty;
        
	[Required]
	[MaxLength(10)]
	public string State { get; set; } = string.Empty;
        
	[Required]
	[MaxLength(10)]
	public string ZipCode { get; set; } = string.Empty;
        
	public CustomerType CustomerType { get; set; } = CustomerType.Residential;
	public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        
	[MaxLength(1000)]
	public string? Notes { get; set; }
        
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public string CreatedBy { get; set; } = string.Empty;
	public string UpdatedBy { get; set; } = string.Empty;
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
using System.ComponentModel.DataAnnotations;

namespace LawnCare.ManagementUI.Models;

/// <summary>
/// An "estimate" from the field is generally in agreement with the customer.  The
/// rep has provided a price for the services requested by the client, and the two
/// are in agreement on the cost.  A best-guess job schedule day may also be provided,
/// but this can be highly dependent on weather, and staff/equipment availability 
/// </summary>
public class JobEstimate
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty; 
    
    //public string TenantId { get; set; } = null!;
    
    [Required(ErrorMessage = "Customer first name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string CustomerFirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Customer last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string CustomerLastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(100, ErrorMessage = "Address cannot exceed 100 characters")]
    public string CustomerAddress1 { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Address line 2 cannot exceed 100 characters")]
    public string CustomerAddress2 { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Address line 3 cannot exceed 100 characters")]
    public string CustomerAddress3 { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    public string CustomerCity { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "State is required")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "State must be exactly 2 characters")]
    public string CustomerState { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ZIP code is required")]
    [StringLength(10, MinimumLength = 5, ErrorMessage = "ZIP code must be between 5 and 10 characters")]
    public string CustomerZip { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string CustomerHomePhone { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Please enter a valid cell phone number")]
    public string CustomerCellPhone { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Scheduled date is required")]
    public DateTimeOffset ScheduledDate { get; set; }
    
    [Required(ErrorMessage = "Estimated cost is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Estimated cost must be greater than 0")]
    public decimal EstimatedCost { get; set; }
    
    [Required(ErrorMessage = "Estimated duration is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Estimated duration must be at least 1 minute")]
    public int EstimatedDuration { get; set; }
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "At least one service item is required")]
    [MinLength(1, ErrorMessage = "At least one service item is required")]
    public List<JobServiceItem> Services { get; set; } = [];
}

public class JobServiceItem
{
    [Required(ErrorMessage = "Service name is required")]
    [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
    public string ServiceName { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Service description cannot exceed 200 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Service cost is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Service cost must be greater than 0")]
    public decimal Cost { get; set; }
    
    [Required(ErrorMessage = "Service duration is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Service duration must be at least 1 minute")]
    public int DurationMinutes { get; set; }
    
    public string Notes { get; set; } = string.Empty;
}

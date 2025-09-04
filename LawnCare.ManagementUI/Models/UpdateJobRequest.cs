using System.ComponentModel.DataAnnotations;

namespace LawnCare.ManagementUI.Models;

public class UpdateJobRequest
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? RequestedServiceDate { get; set; }
    public decimal? JobCost { get; set; }
    public List<ServiceItemRequest>? ServiceItems { get; set; }
    public List<string>? Notes { get; set; }
}

public class ServiceItemRequest
{
    [Required]
    public string ServiceName { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    public string? Comment { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal? Price { get; set; }
}

namespace LawnCare.ManagementUI.Models;

public class ServiceRequest
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PropertyAddress { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string AssignedTechnician { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string PropertySize { get; set; } = string.Empty;
    public string SpecialInstructions { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
}

public class ServiceType
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class JobStatus
{
    public const string Scheduled = "Scheduled";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Rescheduled = "Rescheduled";
}

public class JobPriority
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Urgent = "Urgent";
}

namespace LawnCare.Shared.MessageContracts;

public class FieldEstimate
{
	public string UserId { get; set; } = string.Empty;
	public string TenantId { get; set; } = null!;
	public string CustomerFirstName { get; set; }= string.Empty;
	public string CustomerLastName { get; set; }= string.Empty;
	public string CustomerAddress1 { get; set; }= string.Empty;
	public string CustomerAddress2 { get; set; }= string.Empty;
	public string CustomerAddress3 { get; set; }= string.Empty;
	public string CustomerCity { get; set; }= string.Empty;
	public string CustomerState { get; set; }= string.Empty;
	public string CustomerZip { get; set; }= string.Empty;
	public string CustomerHomePhone { get; set; }= string.Empty;
	public string CustomerCellPhone { get; set; }= string.Empty;
	public string CustomerEmail { get; set; }= string.Empty;
	public DateTimeOffset ScheduledDate { get; set; }
	public decimal EstimatedCost { get; set; }
	public int EstimatedDuration { get; set; }
	public string Description { get; set; }= string.Empty;
	public List<JobServiceItem> Services { get; set; } = [];
}

public record JobServiceItem(string ServiceName, int Quantity, string Comment, decimal Price);

public record ProcessCustomerCommand(Guid TenantId, Guid EstimateId, CustomerInfo Customer);
public record CreateJobCommand(Guid TenantId, Guid EstimateId, Guid CustomerId, JobDetails Job);
public record SendWelcomeEmailCommand(Guid TenantId, Guid CustomerId, CustomerInfo Customer, Guid EstimateId);

// Events
public record EstimateReceivedEvent(Guid TenantId, Guid EstimateId, CustomerInfo Customer, JobDetails Job, string EstimatorId);
public record CustomerFoundEvent(Guid TenantId, Guid EstimateId, Guid CustomerId, bool IsNewCustomer = false);
public record CustomerCreatedEvent(Guid TenantId, Guid EstimateId, Guid CustomerId);
public record CustomerProcessingFailedEvent(Guid TenantId, Guid EstimateId, string Reason);
public record JobCreatedEvent(Guid TenantId, Guid EstimateId, Guid JobId);
public record JobCreationFailedEvent(Guid TenantId, Guid EstimateId, string Reason);
public record WelcomeEmailSentEvent(Guid TenantId, Guid EstimateId, Guid CustomerId);
public record WelcomeEmailFailedEvent(Guid TenantId, Guid EstimateId, Guid CustomerId, string Reason);
public record EstimateProcessingCompletedEvent(Guid TenantId, Guid EstimateId, Guid CustomerId, Guid JobId, bool WasNewCustomer);

// Data Models
public record CustomerInfo(string Email, string FirstName, string LastName, string HomePhone, string CellPhone, Address Address);
public record JobDetails(DateTimeOffset ScheduledDate, decimal EstimatedCost, string Description, JobServiceItem[] ServicesRequested);
public record Address(string Street, string Street2, string Street3, string City, string State, string ZipCode);

public record CreateEstimateRequest(CustomerInfo Customer, JobDetails Job, string EstimatorId);
using MassTransit;

namespace LawnCare.Shared.MessageContracts;

// Job Event Contracts for Email Notifications
public record JobCreatedEmailEvent(
    Guid JobId,
    string CustomerEmail,
    string CustomerFirstName,
    string CustomerLastName,
    string JobDescription,
    decimal EstimatedCost,
    DateTimeOffset ScheduledDate,
    string TechnicianName,
    string PropertyAddress) : CorrelatedBy<Guid>
{
    public Guid CorrelationId => JobId;
}

public record JobUpdatedEmailEvent(
    Guid JobId,
    string CustomerEmail,
    string CustomerFirstName,
    string CustomerLastName,
    string JobDescription,
    decimal EstimatedCost,
    DateTimeOffset? ScheduledDate,
    string TechnicianName,
    string PropertyAddress,
    string UpdateReason,
    string ChangesSummary) : CorrelatedBy<Guid>
{
    public Guid CorrelationId => JobId;
}

public record JobCompletedEmailEvent(
    Guid JobId,
    string CustomerEmail,
    string CustomerFirstName,
    string CustomerLastName,
    string JobDescription,
    decimal ActualCost,
    decimal EstimatedCost,
    DateTimeOffset CompletedDate,
    string TechnicianName,
    string PropertyAddress,
    string CompletionNotes) : CorrelatedBy<Guid>
{
    public Guid CorrelationId => JobId;
}

// Email Data Models
public record EmailRecipient(
    string Email,
    string FirstName,
    string LastName);

public record JobEmailData(
    Guid JobId,
    Guid TenantId,
    string JobDescription,
    decimal Cost,
    DateTimeOffset? ScheduledDate,
    string TechnicianName,
    string PropertyAddress,
    string AdditionalInfo = "");

public record EmailTemplateData(
    string TemplateName,
    Dictionary<string, string> Variables,
    List<string>? Attachments = null);

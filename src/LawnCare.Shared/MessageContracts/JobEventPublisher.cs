using MassTransit;

namespace LawnCare.Shared.MessageContracts;

/// <summary>
/// Helper class for publishing job-related email events
/// This can be used by other services to publish job events that will trigger email notifications
/// </summary>
public static class JobEventPublisher
{
    /// <summary>
    /// Publishes a job created email event
    /// </summary>
    public static async Task PublishJobCreatedEmailEvent(
        this IPublishEndpoint publishEndpoint,
        Guid jobId,
        string customerEmail,
        string customerFirstName,
        string customerLastName,
        string jobDescription,
        decimal estimatedCost,
        DateTimeOffset scheduledDate,
        string technicianName,
        string propertyAddress)
    {
        var @event = new JobCreatedEmailEvent(
            jobId,
            customerEmail,
            customerFirstName,
            customerLastName,
            jobDescription,
            estimatedCost,
            scheduledDate,
            technicianName,
            propertyAddress);

        await publishEndpoint.Publish(@event);
    }

    /// <summary>
    /// Publishes a job updated email event
    /// </summary>
    public static async Task PublishJobUpdatedEmailEvent(
        this IPublishEndpoint publishEndpoint,
        Guid jobId,
        string customerEmail,
        string customerFirstName,
        string customerLastName,
        string jobDescription,
        decimal estimatedCost,
        DateTimeOffset? scheduledDate,
        string technicianName,
        string propertyAddress,
        string updateReason,
        string changesSummary)
    {
        var @event = new JobUpdatedEmailEvent(
            jobId,
            customerEmail,
            customerFirstName,
            customerLastName,
            jobDescription,
            estimatedCost,
            scheduledDate,
            technicianName,
            propertyAddress,
            updateReason,
            changesSummary);

        await publishEndpoint.Publish(@event);
    }

    /// <summary>
    /// Publishes a job completed email event
    /// </summary>
    public static async Task PublishJobCompletedEmailEvent(
        this IPublishEndpoint publishEndpoint,
        Guid jobId,
        string customerEmail,
        string customerFirstName,
        string customerLastName,
        string jobDescription,
        decimal actualCost,
        decimal estimatedCost,
        DateTimeOffset completedDate,
        string technicianName,
        string propertyAddress,
        string completionNotes)
    {
        var @event = new JobCompletedEmailEvent(
            jobId,
            customerEmail,
            customerFirstName,
            customerLastName,
            jobDescription,
            actualCost,
            estimatedCost,
            completedDate,
            technicianName,
            propertyAddress,
            completionNotes);

        await publishEndpoint.Publish(@event);
    }
}

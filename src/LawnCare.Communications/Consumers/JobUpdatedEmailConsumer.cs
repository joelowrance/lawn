using LawnCare.Shared.MessageContracts;

namespace LawnCare.Communications.Consumers;

public class JobUpdatedEmailConsumer : BaseEmailConsumer<JobUpdatedEmailEvent>
{
    public JobUpdatedEmailConsumer(ILogger<JobUpdatedEmailConsumer> logger, IEmailService emailService)
        : base(logger, emailService)
    {
    }

    protected override string GetJobId(JobUpdatedEmailEvent message)
    {
        return message.JobId.ToString();
    }

    protected override async Task ProcessEmailAsync(JobUpdatedEmailEvent message)
    {
        await EmailService.SendJobUpdatedEmailAsync(
            message.CustomerEmail,
            message.CustomerFirstName,
            message.CustomerLastName,
            message.JobDescription,
            message.EstimatedCost,
            message.ScheduledDate,
            message.TechnicianName,
            message.PropertyAddress,
            message.UpdateReason,
            message.ChangesSummary);
    }
}

public class JobUpdatedEmailConsumerDefinition : BaseEmailConsumerDefinition<JobUpdatedEmailConsumer>
{
    public JobUpdatedEmailConsumerDefinition() : base("job-updated-email")
    {
    }
}

using LawnCare.Shared.MessageContracts;

namespace LawnCare.Communications.Consumers;

public class JobCreatedEmailConsumer : BaseEmailConsumer<JobCreatedEmailEvent>
{
    public JobCreatedEmailConsumer(ILogger<JobCreatedEmailConsumer> logger, IEmailService emailService)
        : base(logger, emailService)
    {
    }

    protected override string GetJobId(JobCreatedEmailEvent message)
    {
        return message.JobId.ToString();
    }

    protected override async Task ProcessEmailAsync(JobCreatedEmailEvent message)
    {
        await EmailService.SendJobCreatedEmailAsync(
            message.CustomerEmail,
            message.CustomerFirstName,
            message.CustomerLastName,
            message.JobDescription,
            message.EstimatedCost,
            message.ScheduledDate,
            message.TechnicianName,
            message.PropertyAddress);
    }
}

public class JobCreatedEmailConsumerDefinition : BaseEmailConsumerDefinition<JobCreatedEmailConsumer>
{
    public JobCreatedEmailConsumerDefinition() : base("job-created-email")
    {
    }
}

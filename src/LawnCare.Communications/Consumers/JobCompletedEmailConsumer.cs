using LawnCare.Communications;
using LawnCare.Shared.MessageContracts;
using Polly.Extensions.Http;

namespace LawnCare.Communications.Consumers;

// Job Created Event Consumer

// Job Updated Event Consumer

// Job Completed Event Consumer
public class JobCompletedEmailConsumer : BaseEmailConsumer<JobCompletedEmailEvent>
{
    public JobCompletedEmailConsumer(ILogger<JobCompletedEmailConsumer> logger, IEmailService emailService)
        : base(logger, emailService)
    {
    }

    protected override string GetJobId(JobCompletedEmailEvent message)
    {
        return message.JobId.ToString();
    }

    protected override async Task ProcessEmailAsync(JobCompletedEmailEvent message)
    {
        await EmailService.SendJobCompletedEmailAsync(
            message.CustomerEmail,
            message.CustomerFirstName,
            message.CustomerLastName,
            message.JobDescription,
            message.ActualCost,
            message.EstimatedCost,
            message.CompletedDate,
            message.TechnicianName,
            message.PropertyAddress,
            message.CompletionNotes);
    }
}

public class JobCompletedEmailConsumerDefinition : BaseEmailConsumerDefinition<JobCompletedEmailConsumer>
{
    public JobCompletedEmailConsumerDefinition() : base("job-completed-email")
    {
    }
}

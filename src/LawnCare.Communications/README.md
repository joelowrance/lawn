# Lawn Care Communications Service

This service handles email communications for the Lawn Care platform, including job-related notifications.

## Features

- **Job Email Notifications**: Automatically sends emails for job created, updated, and completed events
- **Resilient Processing**: Built-in retry policies, circuit breakers, and rate limiting
- **Email Tracking**: All emails are logged to the database for audit purposes
- **MassTransit Integration**: Uses RabbitMQ for reliable message processing

## Job Email Events

The service listens for the following job events and sends appropriate email notifications:

### Job Created Email
- **Event**: `JobCreatedEmailEvent`
- **Trigger**: When a new job is created and scheduled
- **Content**: Service details, estimated cost, scheduled date, technician, and property address

### Job Updated Email
- **Event**: `JobUpdatedEmailEvent`
- **Trigger**: When a job is updated (status, schedule, cost, etc.)
- **Content**: Update reason, changes summary, and current service details

### Job Completed Email
- **Event**: `JobCompletedEmailEvent`
- **Trigger**: When a job is marked as completed
- **Content**: Service summary, actual vs estimated cost, completion date, and notes

## Publishing Job Events

To trigger email notifications from other services, use the `JobEventPublisher` helper:

```csharp
// Inject IPublishEndpoint in your service
public class JobService
{
    private readonly IPublishEndpoint _publishEndpoint;
    
    public JobService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task CreateJobAsync(JobDetails job)
    {
        // ... create job logic ...
        
        // Publish job created email event
        await _publishEndpoint.PublishJobCreatedEmailEvent(
            jobId: job.Id,
            tenantId: job.TenantId,
            customerId: job.CustomerId,
            customerEmail: job.CustomerEmail,
            customerFirstName: job.CustomerFirstName,
            customerLastName: job.CustomerLastName,
            jobDescription: job.Description,
            estimatedCost: job.EstimatedCost,
            scheduledDate: job.ScheduledDate,
            technicianName: job.TechnicianName,
            propertyAddress: job.PropertyAddress);
    }
}
```

## Resiliency Features

### Retry Policy
- **Immediate Retries**: 2 attempts
- **Exponential Backoff**: 1s, 2s, 5s, 10s intervals
- **Handled Exceptions**: HttpRequestException, TimeoutException, InvalidOperationException, SmtpException

### Circuit Breaker
- **Tracking Period**: 1 minute
- **Trip Threshold**: 5 failures
- **Active Threshold**: 2 calls
- **Reset Interval**: 1 minute

### Rate Limiting
- **Limit**: 10 messages per minute per consumer
- **Prevents**: Overwhelming the email service

### Timeout
- **Global Timeout**: 2 minutes per message
- **Prevents**: Hanging processes

## Configuration

The service uses the following configuration keys:

```json
{
  "ConnectionStrings": {
    "rabbitmq": "amqp://localhost:5672",
    "communications-connection": "Host=localhost;Database=lawncare_communications;Username=postgres;Password=password",
    "maildev": "smtp://localhost:1025"
  }
}
```

## Email Templates

All job emails use HTML templates with:
- Professional styling
- Customer personalization
- Service details
- Contact information
- Branding consistency

## Monitoring

- All email attempts are logged to the `EmailRecords` table
- Failed emails include failure reasons
- OpenTelemetry integration for distributed tracing
- Circuit breaker status monitoring

## Error Handling

- Failed emails are retried according to the retry policy
- After retry exhaustion, messages go to dead letter queue
- Circuit breaker prevents cascading failures
- All errors are logged with full context

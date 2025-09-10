using MassTransit;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace LawnCare.Communications.Consumers;

/// <summary>
/// Base class for email consumers with common retry and circuit breaker logic
/// </summary>
/// <typeparam name="TMessage">The message type to consume</typeparam>
public abstract class BaseEmailConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    private readonly ILogger _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy CircuitBreakerPolicy;
    protected readonly IEmailService EmailService;

    protected BaseEmailConsumer(ILogger logger, IEmailService emailService)
    {
        _logger = logger;
        EmailService = emailService;

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for {ConsumerType} after {Delay}ms",
                        retryCount, GetType().Name, timespan.TotalMilliseconds);
                });

        // Configure circuit breaker
        CircuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("Circuit breaker opened for {ConsumerType}. Duration: {Duration}",
                        GetType().Name, duration);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset for {ConsumerType}", GetType().Name);
                });
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var message = context.Message;
        _logger.LogInformation("Processing {MessageType} for {JobId}",
            typeof(TMessage).Name, GetJobId(message));

        try
        {
            await CircuitBreakerPolicy.ExecuteAsync(async () =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await ProcessEmailAsync(message);
                });
            });

            _logger.LogInformation("Successfully processed {MessageType} for {JobId}",
                typeof(TMessage).Name, GetJobId(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process {MessageType} for {JobId}",
                typeof(TMessage).Name, GetJobId(message));
            throw; // Re-throw to trigger MassTransit retry/dead letter handling
        }
    }

    /// <summary>
    /// Extract job ID from the message for logging purposes
    /// </summary>
    protected abstract string GetJobId(TMessage message);

    /// <summary>
    /// Process the email sending logic specific to each consumer
    /// </summary>
    protected abstract Task ProcessEmailAsync(TMessage message);
}

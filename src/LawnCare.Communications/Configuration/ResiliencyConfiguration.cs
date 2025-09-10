using MassTransit;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace LawnCare.Communications.Configuration;

public static class ResiliencyConfiguration
{
    public static void ConfigureRetryPolicy(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator
            .Immediate(2)
            .Intervals(1000, 2000, 5000, 10000);
    }

    // Circuit breaker configuration removed for now due to interface complexity
    // Can be re-added later when needed

    public static AsyncRetryPolicy CreateRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger.LogWarning("Retry {RetryCount} after {Delay}ms. Exception: {Exception}",
                        retryCount, timespan.TotalMilliseconds, outcome.Message);
                });
    }

    public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(ILogger logger, string operationName)
    {
        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    logger.LogError("Circuit breaker opened for {OperationName}. Duration: {Duration}. Exception: {Exception}",
                        operationName, duration, exception.Message);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset for {OperationName}", operationName);
                });
    }

    // HTTP resilience method removed for now due to interface complexity
    // Can be re-added later when needed
}

using LawnCare.Communications.Configuration;
using MassTransit;

namespace LawnCare.Communications.Consumers;

/// <summary>
/// Base consumer definition for email consumers with common configuration
/// </summary>
/// <typeparam name="TConsumer">The consumer type</typeparam>
public abstract class BaseEmailConsumerDefinition<TConsumer> : ConsumerDefinition<TConsumer>
    where TConsumer : class, IConsumer
{
    protected BaseEmailConsumerDefinition(string endpointName, int concurrentMessageLimit = 5)
    {
        EndpointName = endpointName;
        ConcurrentMessageLimit = concurrentMessageLimit;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, 
        IConsumerConfigurator<TConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // Configure retry policy using centralized configuration
        endpointConfigurator.UseMessageRetry(ResiliencyConfiguration.ConfigureRetryPolicy);

        // Configure rate limiting
        endpointConfigurator.UseRateLimit(10, TimeSpan.FromMinutes(1));

        // Configure timeout
        endpointConfigurator.UseTimeout(t => t.Timeout = TimeSpan.FromMinutes(2));

        // Allow derived classes to add custom configuration
        ConfigureConsumerCustom(endpointConfigurator, consumerConfigurator, context);
    }

    /// <summary>
    /// Override this method to add custom configuration for specific consumers
    /// </summary>
    protected virtual void ConfigureConsumerCustom(IReceiveEndpointConfigurator endpointConfigurator, 
        IConsumerConfigurator<TConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // Default implementation does nothing - can be overridden by derived classes
    }
}

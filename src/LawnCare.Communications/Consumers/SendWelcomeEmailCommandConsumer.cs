using LawnCare.Communications;
using LawnCare.Shared.MessageContracts;
using MassTransit;

public class SendWelcomeEmailCommandConsumer : IConsumer<SendWelcomeEmailCommand>
{
    private readonly ILogger<ProcessCustomerCommand> _logger;
    private readonly IEmailService _emailService;

    private readonly IPublishEndpoint _publishEndpoint;
    //private readonly ICustomerService _customerService;

    public SendWelcomeEmailCommandConsumer(ILogger<ProcessCustomerCommand> logger, IEmailService emailService, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _emailService = emailService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<SendWelcomeEmailCommand> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _logger.LogInformation("Received customer processing message for {CustomerEmail}",
            context.Message.Customer.Email);

        await _emailService.SendWelcomeEmail(context.Message.Customer);
        //await _publishEndpoint.Publish(new WelcomeEmailSentEvent(context.Message.TenantId, context.Message.EstimateId, context.Message.CustomerId));
        _logger.LogInformation("Successfully processed customer message for {CustomerEmail}",
            context.Message.Customer.Email);
    }
}

internal class SendWelcomeEmailCommandConsumerDefinition : ConsumerDefinition<SendWelcomeEmailCommandConsumer>
{
    public SendWelcomeEmailCommandConsumerDefinition()
    {
        EndpointName = "communications-api";
        ConcurrentMessageLimit = 10;

    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SendWelcomeEmailCommandConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
    }
}

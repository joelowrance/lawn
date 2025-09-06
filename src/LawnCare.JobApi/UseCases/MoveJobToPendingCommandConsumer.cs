using LawnCare.JobApi.Domain.Services;
using LawnCare.Shared.MessageContracts;

using MassTransit;

namespace LawnCare.JobApi.UseCases
{
	public class MoveJobToPendingCommandConsumer : IConsumer<MoveJobToPendingCommand>
	{
		readonly ILogger<AcceptEstimateCommandHandler> _logger;
		readonly IPublishEndpoint _publishEndpoint;
		private readonly IJobApplicationService  _jobApplicationService;

		public MoveJobToPendingCommandConsumer(ILogger<AcceptEstimateCommandHandler> logger, IPublishEndpoint publishEndpoint, IJobApplicationService jobApplicationService)
		{
			_logger = logger;
			_publishEndpoint = publishEndpoint;
			_jobApplicationService = jobApplicationService;
		}

		public async Task Consume(ConsumeContext<MoveJobToPendingCommand> context)
		{
			var response = await _jobApplicationService.SetPending(context.Message.JobId, context.Message.CustomerId);
			_logger.LogInformation("Moved job {JobId} to pending", context.Message.JobId);

			var jobCreated = new JobCreatedEvent(response.Id, Guid.Empty, response.Id, context.Message.CustomerId);
			await _publishEndpoint.Publish(jobCreated);
		}
	}
	
	internal class MoveJobToPendingCommandConsumerDefinition : ConsumerDefinition<MoveJobToPendingCommandConsumer>
	{
		public MoveJobToPendingCommandConsumerDefinition()
		{
			EndpointName = "job-api";
			ConcurrentMessageLimit = 10;
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<MoveJobToPendingCommandConsumer> consumerConfigurator,
			IRegistrationContext context)
		{
			endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
		}
	}

}
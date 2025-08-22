using LawnCare.Shared.MessageContracts;

using MassTransit;

namespace LawnCare.Shared.MessageContracts
{
	public class EstimateProcessingSaga : MassTransitStateMachine<EstimateProcessingState>
	{
		public State? ProcessingCustomer { get; private set; }
		public State? CreatingJob { get; private set; }
		public State? SendingWelcomeEmail { get; private set; }
		public State? Completed { get; private set; }
		public State? Failed { get; private set; }

		public Event<EstimateReceivedEvent>? EstimateReceived { get; private set; }
		public Event<CustomerFoundEvent>? CustomerFound { get; private set; }
		public Event<CustomerCreatedEvent>? CustomerCreated { get; private set; }
		public Event<CustomerProcessingFailedEvent>? CustomerProcessingFailed { get; private set; }
		public Event<JobCreatedEvent>? JobCreated { get; private set; }
		public Event<JobCreationFailedEvent>? JobCreationFailed { get; private set; }
		public Event<WelcomeEmailSentEvent>? WelcomeEmailSent { get; private set; }
		public Event<WelcomeEmailFailedEvent>? WelcomeEmailFailed { get; private set; }

		public EstimateProcessingSaga()
		{
			InstanceState(x => x.CurrentState);
        
			Event(() => EstimateReceived, x => x.CorrelateById(m => m.Message.EstimateId));
			Event(() => CustomerFound, x => x.CorrelateById(m => m.Message.EstimateId));
			Event(() => CustomerCreated, x => x.CorrelateById(m => m.Message.EstimateId));
			Event(() => CustomerProcessingFailed, x => x.CorrelateById(m => m.Message.EstimateId));
			Event(() => JobCreated, x => x.CorrelateById(m => m.Message.EstimateId));
			Event(() => JobCreationFailed, x => x.CorrelateById(m => m.Message.EstimateId));
			Event(() => WelcomeEmailSent, x => x.CorrelateById(m => m.Message.EstimateId));
			Event(() => WelcomeEmailFailed, x => x.CorrelateById(m => m.Message.EstimateId));

			Initially(
				When(EstimateReceived)
					.Then(context =>
					{
						context.Saga.EstimateId = context.Message.EstimateId; //kill this fucking thing
						context.Saga.JobId = context.Message.EstimateId;
						context.Saga.TenantId = context.Message.TenantId;
						context.Saga.CustomerInfo = context.Message.Customer;
						context.Saga.JobDetails = context.Message.Job;
						context.Saga.EstimatorId = context.Message.EstimatorId;
						context.Saga.CreatedAt = DateTime.UtcNow;
					})
					.Publish(context => new ProcessCustomerCommand(
						context.Message.TenantId,
						context.Message.EstimateId,
						context.Message.Customer))
					.TransitionTo(ProcessingCustomer)
			);

			During(ProcessingCustomer,
				When(CustomerFound)
					.Then(context => 
					{
						context.Saga.CustomerId = context.Message.CustomerId;
						context.Saga.IsNewCustomer = context.Message.IsNewCustomer;
					})
					.Publish(context => new MoveJobToPendingCommand(
						context.Saga.JobId!.Value,
						context.Saga.CustomerId!.Value))
					.TransitionTo(CreatingJob),

				When(CustomerCreated)
					.Then(context => 
					{
						context.Saga.CustomerId = context.Message.CustomerId;
						context.Saga.IsNewCustomer = true;
					})
					.Publish(context => new MoveJobToPendingCommand(
						context.Saga.JobId!.Value,
						context.Saga.CustomerId!.Value))
					.TransitionTo(CreatingJob),
            
				When(CustomerProcessingFailed)
					.Then(context => context.Saga.ErrorReason = context.Message.Reason)
					.TransitionTo(Failed)
			);

			During(CreatingJob,
				When(JobCreated)
					.Then(context => context.Saga.JobId = context.Message.JobId)
					.If(context => context.Saga.IsNewCustomer,
						binder => binder
							.Publish(context => new SendWelcomeEmailCommand(
								context.Saga.TenantId,
								context.Saga.CustomerId!.Value,
								context.Saga.CustomerInfo,
								context.Saga.EstimateId))
							.TransitionTo(SendingWelcomeEmail))
					.If(context => !context.Saga.IsNewCustomer,
						binder => binder
							.Then(context => context.Saga.CompletedAt = DateTime.UtcNow)
							.Publish(context => new EstimateProcessingCompletedEvent(
								context.Saga.TenantId,
								context.Saga.EstimateId,
								context.Saga.CustomerId!.Value,
								context.Message.JobId,
								false))
							.TransitionTo(Completed)),
            
				When(JobCreationFailed)
					.Then(context => context.Saga.ErrorReason = context.Message.Reason)
					.TransitionTo(Failed)
			);

			During(SendingWelcomeEmail,
				When(WelcomeEmailSent)
					.Then(context => context.Saga.CompletedAt = DateTime.UtcNow)
					.Publish(context => new EstimateProcessingCompletedEvent(
						context.Saga.TenantId,
						context.Saga.EstimateId,
						context.Saga.CustomerId!.Value,
						context.Saga.JobId!.Value,
						true))
					.TransitionTo(Completed),

				When(WelcomeEmailFailed)
					.Then(context => 
					{
						context.Saga.WelcomeEmailError = context.Message.Reason;
						context.Saga.CompletedAt = DateTime.UtcNow;
					})
					.Publish(context => new EstimateProcessingCompletedEvent(
						context.Saga.TenantId,
						context.Saga.EstimateId,
						context.Saga.CustomerId!.Value,
						context.Saga.JobId!.Value,
						true)) // Still complete even if email fails
					.TransitionTo(Completed)
			);
		}
	}
}

public class EstimateProcessingState : SagaStateMachineInstance
{
	public Guid CorrelationId { get; set; }
	public string CurrentState { get; set; } = null!;
	public Guid EstimateId { get; set; }
	public Guid TenantId { get; set; }
	public Guid? CustomerId { get; set; }
	public Guid? JobId { get; set; }
	public CustomerInfo CustomerInfo { get; set; } = null!;
	public JobDetails JobDetails { get; set; } = null!;
	public string EstimatorId { get; set; } = null!;
	public bool IsNewCustomer { get; set; }
	public string? ErrorReason { get; set; }
	public string? WelcomeEmailError { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? CompletedAt { get; set; }
	
	//Shadow property
	public byte[]? Version { get; set; } // For optimistic concurrency
}
//using JobService.Infrastructure.Persistence;

using JobService.Infrastructure.Persistence;

using LawnCare.JobApi.Domain.Common;

using MassTransit.Mediator;

namespace LawnCare.JobApi.Infrastructure.Database;

public class UnitOfWork : IUnitOfWork
{
	private readonly JobDbContext _context;
	private readonly MassTransit.Mediator.IMediator _mediator;
	private readonly ILogger<UnitOfWork> _logger;

	public UnitOfWork(JobDbContext context, 
		IMediator mediator, 
		ILogger<UnitOfWork> logger)
	{
		_context = context;
		_mediator = mediator;
		_logger = logger;
	}

	public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		// Get all aggregates with domain events
		var aggregatesWithEvents = _context.ChangeTracker
			.Entries<AggregateRoot>()
			.Where(x => x.Entity.DomainEvents.Any())
			.Select(x => x.Entity)
			.ToList();

		// Collect all domain events
		var domainEvents = aggregatesWithEvents
			.SelectMany(x => x.DomainEvents)
			.ToList();

		// Clear events from aggregates (important to prevent re-processing)
		aggregatesWithEvents.ForEach(aggregate => aggregate.ClearDomainEvents());

		// Save changes to database first
		var result = await _context.SaveChangesAsync(cancellationToken);

		// Then dispatch domain events (after successful save)
		foreach (var domainEvent in domainEvents)
		{
			try
			{
				_logger.LogDebug("Dispatching domain event: {EventType}", domainEvent.GetType().Name);
				await _mediator.Publish(domainEvent, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error dispatching domain event: {EventType}", domainEvent.GetType().Name);
				// Consider your error handling strategy here
				// You might want to publish to a dead letter queue or retry
			}
		}

		return result;
	}
}
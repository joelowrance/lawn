namespace LawnCare.CoreApi.Domain.Common;

/// <summary>
/// Represents the root entity of a domain aggregate in Domain-Driven Design (DDD).
/// An aggregate is a cluster of related objects that are treated as a single unit
/// for data changes. The aggregate root:
/// - Defines the aggregate's boundary and is the only entry point from the outside.
/// - Enforces invariants and business rules for the entire aggregate.
/// - Controls creation, modification, and deletion of child entities/value objects.
/// - Serves as the transactional consistency boundary (changes within a single aggregate
///   should be committed atomically).
/// - Is typically the target of repository operations (load/save by root identity).
/// - May capture and publish domain events that describe state changes or decisions,
///   allowing side effects to be handled elsewhere.
/// 
/// Guidance:
/// - Expose behavior-rich methods on the root that express intent, rather than leaking
///   internal structure.
/// - Avoid exposing direct references to child entities outside the aggregate boundary.
/// - Keep cross-aggregate interactions via IDs or domain events to preserve autonomy.
/// </summary>

public abstract class AggregateRoot 
{
	private readonly List<IDomainEvent> _domainEvents = new();

	public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

	protected void AddDomainEvent(IDomainEvent domainEvent)
	{
		_domainEvents.Add(domainEvent);
	}

	public void ClearDomainEvents()
	{
		_domainEvents.Clear();
	}
}

public interface IAuditable
{
	public DateTimeOffset CreatedAt { get; internal set; }
	public DateTimeOffset UpdatedAt { get; internal set; }
}
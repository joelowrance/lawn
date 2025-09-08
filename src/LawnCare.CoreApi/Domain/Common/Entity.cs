namespace LawnCare.CoreApi.Domain.Common;

/// <summary>
/// Represents a Domain-Driven Design (DDD) Entity: an object whose primary distinction is a
/// stable, unique identity that persists through changes over time.
///
/// Key characteristics:
/// - Identified by a unique ID; equality is based on identity, not the full set of attributes.
/// - Encapsulates state and behavior; methods enforce invariants local to the entity.
/// - State is typically mutable, but transitions should be validated through behavior-rich methods.
/// - May raise domain events to describe significant state changes.
/// - Persisted and retrieved via a repository using the entity’s identity.
///
/// Guidance:
/// - Keep the identity immutable once assigned; do not allow it to change during the lifecycle.
/// - Prefer intent-revealing methods over public setters; guard invariants internally.
/// - Distinguish from value objects (which are immutable and compared by value).
/// - Implement equality and hashing based on the identity, handling transient (not-yet-persisted)
///   entities carefully.
/// </summary>

public abstract class Entity
{
	public Guid Id { get; protected set; }  // Raw Guid

	public override bool Equals(object? obj)
	{
		return obj is Entity other && GetType() == other.GetType() && GetHashCode() == other.GetHashCode();
	}

	public override int GetHashCode()
	{
		return GetType().GetHashCode();
	}
}

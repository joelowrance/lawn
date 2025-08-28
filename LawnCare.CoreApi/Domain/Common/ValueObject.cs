namespace LawnCare.CoreApi.Domain.Common;

/// <summary>
/// Represents a Domain-Driven Design (DDD) Value Object: a small, immutable type whose
/// identity is defined entirely by the values of its attributes rather than by a unique ID.
/// 
/// Key characteristics:
/// - Equality is structural: two instances are equal if all their defining components are equal.
/// - No identity or lifecycle tracking; instances are freely replaceable.
/// - Encapsulates validation and behavior relevant to the value it models.
/// - Immutability by design promotes safety, predictability, and thread-friendliness.
/// 
/// Guidance:
/// - Keep properties get-only and enforce invariants at creation time (factory/constructor).
/// - Include only stable, meaningful attributes in equality components.
/// - Favor rich, intention-revealing methods over exposing raw data.
/// - Use value objects to model concepts like money, dates ranges, addresses, and measurements,
///   especially where identity is not meaningful.
/// </summary>

public abstract class ValueObject
{
	protected abstract IEnumerable<object> GetEqualityComponents();

	public override bool Equals(object? obj)
	{
		if (obj == null || obj.GetType() != GetType())
		{
			return false;
		}

		ValueObject other = (ValueObject)obj;
		return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
	}

	public override int GetHashCode()
	{
		return GetEqualityComponents()
			.Select(x => x?.GetHashCode() ?? 0)
			.Aggregate((x, y) => x ^ y);
	}
}
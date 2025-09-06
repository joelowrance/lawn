﻿namespace LawnCare.JobApi.Domain.Common;

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
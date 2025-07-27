using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects;

public class JobId : ValueObject
{
	public Guid Value { get; }

	private JobId(Guid value)
	{
		Value = value;
	}

	public static JobId Create() => new(Guid.NewGuid());
        
	public static JobId From(Guid value) => new(value);

	protected override IEnumerable<object> GetEqualityComponents()
	{
		yield return Value;
	}

	public override string ToString() => Value.ToString();
}
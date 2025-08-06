using LawnCare.JobApi.Domain.Common;
using LawnCare.Shared;

namespace LawnCare.JobApi.Domain.ValueObjects;

public class JobId : ValueObject
{
	public Guid Value { get; }

	private JobId(Guid value)
	{
		Value = value;
	}

	public static JobId Create() => new(GuidHelper.NewId());
        
	public static JobId From(Guid value) => new(value);

	protected override IEnumerable<object> GetEqualityComponents()
	{
		yield return Value;
	}

	public override string ToString() => Value.ToString();
}
using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects;

public class EstimatedDuration : ValueObject
{
	public TimeSpan Duration { get; }
	public string Unit { get; }

	public EstimatedDuration(int hours)
	{
		if (hours < 0 || hours > 40)
			throw new ArgumentException("Duration cannot be negative");

		Duration = new TimeSpan(hours, 0, 0);
		Unit = "hours";
	}

	public double TotalHours => Duration.TotalHours;

	protected override IEnumerable<object> GetEqualityComponents()
	{
		yield return Duration;
	}

	public override string ToString() => $"{TotalHours:F1} hours";
}
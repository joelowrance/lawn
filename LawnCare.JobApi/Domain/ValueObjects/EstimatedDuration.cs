using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects;

public class EstimatedDuration : ValueObject
{
	public TimeSpan Duration { get; }
	public string Unit { get; }

	public EstimatedDuration(int hours, int minutes = 0)
	{
		if (hours < 0 || minutes < 0)
			throw new ArgumentException("Duration cannot be negative");

		Duration = new TimeSpan(hours, minutes, 0);
		Unit = "hours";
	}

	public static EstimatedDuration FromHours(double hours) => 
		new((int)hours, (int)((hours - (int)hours) * 60));

	public double TotalHours => Duration.TotalHours;

	protected override IEnumerable<object> GetEqualityComponents()
	{
		yield return Duration;
	}

	public override string ToString() => $"{TotalHours:F1} hours";
}
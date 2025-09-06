using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects
{
	public class TechnicianId : ValueObject
	{
		public Guid Value { get; }
		private TechnicianId(Guid value) => Value = value;
		public static TechnicianId From(Guid value) => new(value);
		protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
	}
}
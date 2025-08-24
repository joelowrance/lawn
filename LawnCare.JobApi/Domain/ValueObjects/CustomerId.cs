using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects
{
	public class CustomerId : ValueObject
	{
		public Guid Value { get; }
		private CustomerId(Guid value) => Value = value;
		public static CustomerId From(Guid value) => new(value);
		protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
	}

}
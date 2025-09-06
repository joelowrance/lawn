using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects
{
	public class AddressId : ValueObject
	{
		public Guid Value { get; }
		private AddressId(Guid value) => Value = value;
		public static AddressId From(Guid value) => new(value);
		protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
	}
}
using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects
{
	public class TenantId : ValueObject
	{
		public Guid Value { get; }
		private TenantId(Guid value) => Value = value;
		public static TenantId From(Guid value) => new(value);
		protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
	}
}
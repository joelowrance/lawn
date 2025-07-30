using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects
{
	public class TenantId : ValueObject
	{
		public Guid Value { get; }
		private TenantId(Guid value) => Value = value;
		public static TenantId From(Guid value) => new(value);
		public static TenantId From(String guidRepresentation) => 
			Guid.TryParse(guidRepresentation, out var guid) ? From(guid) : throw new ArgumentException("Invalid Guid representation", nameof(guidRepresentation));
		protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
	}
}
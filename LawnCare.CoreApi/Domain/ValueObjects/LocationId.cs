using LawnCare.CoreApi.Domain.Common;
using LawnCare.Shared;

namespace LawnCare.CoreApi.Domain.Entities
{
	public class LocationId : ValueObject
	{
		public Guid Value { get; }

		private LocationId(Guid value)
		{
			Value = value;
		}

		public static LocationId Create() => new(GuidHelper.NewId());
        
		public static LocationId From(Guid value) => new(value);

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Value;
		}

		public override string ToString() => Value.ToString();
	}
}
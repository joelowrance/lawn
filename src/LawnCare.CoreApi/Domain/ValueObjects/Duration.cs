using LawnCare.CoreApi.Domain.Common;

namespace LawnCare.CoreApi.Domain.Entities
{
	public class Duration : ValueObject
	{
		public TimeSpan Value { get; }
		protected override IEnumerable<object> GetEqualityComponents()
		{
			throw new NotImplementedException();
		}
	}
}
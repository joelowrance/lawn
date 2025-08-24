using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.Entities
{

	
	public class Address : Entity
	{
		public string Street1 { get; }
		public string Street2 { get; }
		public string Street3 { get; }
		public string City { get; }
		public string State { get; }
		public string ZipCode { get; }
		public decimal? Latitude { get; }
		public decimal? Longitude { get; }

		private Address()
		{
			Street1 = null!;
			Street2 = null!;
			Street3 = null!;
			City = null!;
			State = null!;
			ZipCode = null!;
		} // EF Core constructor

		public Address(string street1, string street2 , string street3, string city, string state, string zipCode 
			, decimal? latitude = null, decimal? longitude = null)
		{
			Street1 = street1 ?? throw new ArgumentNullException(nameof(street1));
			Street2 = street2;
			Street3 = street3;
			City = city ?? throw new ArgumentNullException(nameof(city));
			State = state ?? throw new ArgumentNullException(nameof(state));
			ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
			Latitude = latitude;
			Longitude = longitude;
		}

		public string FullAddress
		{
			get
			{
				var add = Street1;
				if (!string.IsNullOrWhiteSpace(Street2))
				{
					add += ", " + Street2;
				}
			
				if (!string.IsNullOrWhiteSpace(Street3))
				{
					add += ", " + Street3;
				}

				add += $"{City}, {State} {ZipCode}";
				return add;
			}	
		}
	
		

		// protected override IEnumerable<object> GetEqualityComponents()
		// {
		// 	yield return Street1;
		// 	yield return Street2;
		// 	yield return Street3;
		// 	yield return City;
		// 	yield return State;
		// 	yield return ZipCode;
		// }	
	}
}
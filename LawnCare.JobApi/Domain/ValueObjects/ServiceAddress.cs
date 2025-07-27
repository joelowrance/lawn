using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects;

public class ServiceAddress : ValueObject
{
	public string Street { get; }
	public string City { get; }
	public string State { get; }
	public string ZipCode { get; }
	public string? ApartmentUnit { get; }
	public decimal? Latitude { get; }
	public decimal? Longitude { get; }

	public ServiceAddress(string street, string city, string state, string zipCode, 
		string? apartmentUnit = null, decimal? latitude = null, decimal? longitude = null)
	{
		Street = street ?? throw new ArgumentNullException(nameof(street));
		City = city ?? throw new ArgumentNullException(nameof(city));
		State = state ?? throw new ArgumentNullException(nameof(state));
		ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
		ApartmentUnit = apartmentUnit;
		Latitude = latitude;
		Longitude = longitude;
	}

	public string FullAddress => ApartmentUnit != null 
		? $"{Street}, {ApartmentUnit}, {City}, {State} {ZipCode}"
		: $"{Street}, {City}, {State} {ZipCode}";

	protected override IEnumerable<object> GetEqualityComponents()
	{
		yield return Street;
		yield return City;
		yield return State;
		yield return ZipCode;
		yield return ApartmentUnit ?? string.Empty;
	}
}
using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.ValueObjects;

public class ServiceType : ValueObject
{
	public string Category { get; }  // "LawnCare" or "TreeSpecialist"
	public string ServiceName { get; }
	public string Description { get; }

	public ServiceType(string category, string serviceName, string description)
	{
		if (!IsValidCategory(category))
			throw new ArgumentException("Invalid service category", nameof(category));

		Category = category;
		ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
		Description = description ?? throw new ArgumentNullException(nameof(description));
	}

	private static bool IsValidCategory(string category)
	{
		return category is "LawnCare" or "TreeSpecialist";
	}

	// Factory methods for common services
	public static ServiceType LawnMowing() => 
		new("LawnCare", "Lawn Mowing", "Regular grass cutting and maintenance");
        
	public static ServiceType TreeTrimming() => 
		new("TreeSpecialist", "Tree Trimming", "Professional tree trimming and pruning");
        
	public static ServiceType TreeRemoval() => 
		new("TreeSpecialist", "Tree Removal", "Complete tree removal service");
        
	public static ServiceType LandscapeMaintenance() => 
		new("LawnCare", "Landscape Maintenance", "Complete landscape maintenance service");

	protected override IEnumerable<object> GetEqualityComponents()
	{
		yield return Category;
		yield return ServiceName;
	}
}
using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.Entities
{
	public class JobRequirement : Entity
	{
		public string RequirementType { get; private set; }
		public string Description { get; private set; }
		public bool IsRequired { get; private set; }
		public bool IsFulfilled { get; private set; }

		public JobRequirement(string requirementType, string description, bool isRequired = true)
		{
			RequirementType = requirementType ?? throw new ArgumentNullException(nameof(requirementType));
			Description = description ?? throw new ArgumentNullException(nameof(description));
			IsRequired = isRequired;
			IsFulfilled = false;
		}

		public void MarkAsFulfilled()
		{
			IsFulfilled = true;
		}
	}
}
using LawnCare.JobApi.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.JobApi.Infrastructure.Database;

internal class JobRequirementConfiguration : IEntityTypeConfiguration<JobRequirement>
{
	public void Configure(EntityTypeBuilder<JobRequirement> builder)
	{
		builder.ToTable("JobRequirements", schema: "JobService");

		// Primary key
		builder.HasKey(jr => jr.Id);

		// Properties
		builder.Property(jr => jr.RequirementType)
			.IsRequired()
			.HasMaxLength(100)
			.HasComment("Type of requirement (Equipment, Material, Skill, etc.)");

		builder.Property(jr => jr.Description)
			.IsRequired()
			.HasMaxLength(500)
			.HasComment("Detailed description of the requirement");

		builder.Property(jr => jr.IsRequired)
			.IsRequired()
			.HasDefaultValue(true)
			.HasComment("Whether this requirement is mandatory");

		builder.Property(jr => jr.IsFulfilled)
			.IsRequired()
			.HasDefaultValue(false)
			.HasComment("Whether this requirement has been fulfilled");

		// Foreign key to Job (shadow property)
		builder.Property<Guid>("JobId")
			.IsRequired()
			.HasComment("Foreign key to the Job");

		// Indexes
		builder.HasIndex("JobId")
			.HasDatabaseName("IX_JobRequirements_JobId");

		builder.HasIndex(jr => jr.RequirementType)
			.HasDatabaseName("IX_JobRequirements_RequirementType");

		builder.HasIndex(jr => new { jr.IsRequired, jr.IsFulfilled })
			.HasDatabaseName("IX_JobRequirements_IsRequired_IsFulfilled");
	}
}
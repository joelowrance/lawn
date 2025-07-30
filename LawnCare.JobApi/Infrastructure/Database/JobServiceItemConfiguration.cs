using LawnCare.JobApi.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.JobApi.Infrastructure.Database
{
	internal class JobServiceItemConfiguration : IEntityTypeConfiguration<JobServiceItem>
	{
		public void Configure(EntityTypeBuilder<JobServiceItem> builder)
		{
			builder.ToTable("JoeServiceItems", schema: "JobService");

			// Primary key
			builder.HasKey(jr => jr.Id);

			// Properties
			builder.Property(jr => jr.ServiceName)
				.IsRequired()
				.HasMaxLength(255)
				.HasComment("Type of service (mowing, tree removal, etc.)");

			builder.Property(jr => jr.Comment)
				.IsRequired()
				.HasMaxLength(2048)
				.HasComment("Detailed description of the requirement");

			builder.Property(jr => jr.Quantity)
				.IsRequired()
				.HasDefaultValue(1)
				.HasComment("Number of units");
			
			builder.Property(jr => jr.Price)
				.IsRequired()
				.HasPrecision(7, 2)
				.HasDefaultValue(1)
				.HasComment("Number of units");


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
				.HasDatabaseName("IX_JobServiceItem_JobId");

			builder.HasIndex(jr => jr.ServiceName)
				.HasDatabaseName("IX_JobServiceItem_ServiceType");

			builder.HasIndex(jr => new { jr, jr.IsFulfilled })
				.HasDatabaseName("IX_JobServiceItem_IsFulfilled");
		}
	}
}

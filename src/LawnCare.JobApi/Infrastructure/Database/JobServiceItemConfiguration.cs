using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.JobApi.Infrastructure.Database;

internal class JobServiceItemConfiguration : IEntityTypeConfiguration<JobServiceItem>
{
	public void Configure(EntityTypeBuilder<JobServiceItem> builder)
	{
		builder.ToTable("JobServiceItems", schema: "JobService");

		// Primary key
		builder.HasKey(js => js.Id);

		// Properties
		builder.Property(js => js.ServiceName)
			.IsRequired()
			.HasMaxLength(200)
			.HasComment("Name of the service provided");

		builder.Property(js => js.Quantity)
			.IsRequired()
			.HasDefaultValue(1)
			.HasComment("Quantity of service provided");

		builder.Property(js => js.Price)
			.IsRequired()
			.HasPrecision(7, 2)
			.HasComment("Price per unit of service");

		builder.Property(js => js.Comment)
			.IsRequired(false)
			.HasMaxLength(500)
			.HasComment("Optional comment about the service");

		builder.Property(js => js.IsFulfilled)
			.IsRequired()
			.HasDefaultValue(false)
			.HasComment("Whether this service has been fulfilled");

		// Foreign key to Job using JobId value object
		builder.Property(js => js.JobId)
			.HasConversion(
				v => v!.Value,
				v => JobId.From(v))
			.IsRequired()
			.HasComment("Foreign key to the Job");

		// Indexes
		builder.HasIndex(js => js.JobId)
			.HasDatabaseName("IX_JobServiceItems_JobId");

		builder.HasIndex(js => js.ServiceName)
			.HasDatabaseName("IX_JobServiceItems_ServiceName");

		builder.HasIndex(js => js.IsFulfilled)
			.HasDatabaseName("IX_JobServiceItems_IsFulfilled");
	}
}

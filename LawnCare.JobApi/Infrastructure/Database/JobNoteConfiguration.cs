using LawnCare.JobApi.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.JobApi.Infrastructure.Database;

internal class JobNoteConfiguration : IEntityTypeConfiguration<JobNote>
{
	public void Configure(EntityTypeBuilder<JobNote> builder)
	{
		builder.ToTable("JobNotes", schema: "JobService");

		// Primary key
		builder.HasKey(jn => jn.Id);

		// Properties
		builder.Property(jn => jn.Author)
			.IsRequired()
			.HasMaxLength(200)
			.HasComment("Author of the note (technician, customer, system, etc.)");

		builder.Property(jn => jn.Content)
			.IsRequired()
			.HasMaxLength(2000)
			.HasComment("Content of the note");

		builder.Property(jn => jn.CreatedAt)
			.IsRequired()
			.HasDefaultValueSql("GETUTCDATE()")
			.HasComment("Timestamp when note was created");

		// Foreign key to Job (shadow property) - using raw Guid
		builder.Property<Guid>("JobId")
			.IsRequired()
			.HasComment("Foreign key to the Job");

		// Indexes
		builder.HasIndex("JobId")
			.HasDatabaseName("IX_JobNotes_JobId");

		builder.HasIndex(jn => jn.CreatedAt)
			.HasDatabaseName("IX_JobNotes_CreatedAt");

		builder.HasIndex(jn => jn.Author)
			.HasDatabaseName("IX_JobNotes_Author");
	}
}
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
			.HasDefaultValueSql("CURRENT_TIMESTAMP")
			.HasComment("Timestamp when note was created");

		// Configure JobId conversion
		var jobIdConverter = new ValueConverter<JobId?, Guid?>(
			v => v != null ? v.Value : null,
			v => v.HasValue ? JobId.From(v.Value) : null);

		// Foreign key to Job using JobId property
		builder.Property(jn => jn.JobId)
			.HasConversion(jobIdConverter)
			.IsRequired()
			.HasComment("Foreign key to the Job");

		// Indexes
		builder.HasIndex(jn => jn.JobId)
			.HasDatabaseName("IX_JobNotes_JobId");

		builder.HasIndex(jn => jn.CreatedAt)
			.HasDatabaseName("IX_JobNotes_CreatedAt");

		builder.HasIndex(jn => jn.Author)
			.HasDatabaseName("IX_JobNotes_Author");
	}
}
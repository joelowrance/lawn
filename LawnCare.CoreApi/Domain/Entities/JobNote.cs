using LawnCare.CoreApi.Domain.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.CoreApi.Domain.Entities
{
	public class JobNote : Entity, IAuditable
	{
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
		public string Note { get; private set; } = string.Empty;
		public JobId JobId { get; internal set; } = null!;

		// EF core
		private JobNote()
		{
		}

		public JobNote(JobId jobId, string note)
		{
			JobId = jobId ?? throw new ArgumentNullException(nameof(jobId));
			Note = note ?? throw new ArgumentNullException(nameof(note));;
		}
	}
	
	public class JobNoteConfiguration : IEntityTypeConfiguration<JobNote>
	{
		public void Configure(EntityTypeBuilder<JobNote> builder)
		{
			builder.ToTable("JobNotes", "public");
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Note)
				.IsRequired()
				.HasMaxLength(-1)
				.HasComment("The note");
			
			// Indexes
			builder.HasIndex(js => js.JobId)
				.HasDatabaseName("IX_JobNotes_JobId");
		}
	}

}
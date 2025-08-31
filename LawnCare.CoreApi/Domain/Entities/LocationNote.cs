using LawnCare.CoreApi.Domain.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.CoreApi.Domain.Entities
{
	public class LocationNote : Entity, IAuditable
	{
		public string Text { get; private set; } =null!;
		public string CreatedBy { get; private set; } = null!;
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
		public  LocationId LocationId { get; private set; } = null!;

		private LocationNote()
		{
		}

		public LocationNote(LocationId locationId, string text, string createdBy)
		{
			Text = text;
			CreatedBy = createdBy;
		}
	}

	public class LocationNoteConfiguration : IEntityTypeConfiguration<LocationNote>
	{
		public void Configure(EntityTypeBuilder<LocationNote> builder)
		{
			builder.ToTable("location_notes");
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Text).IsRequired()
				.HasMaxLength(-1).HasComment("The note");
			builder.Property(x => x.CreatedBy).IsRequired();
			builder.Property(x => x.CreatedAt).IsRequired();
			builder.Property(x => x.UpdatedAt).IsRequired();
			
			builder.HasIndex(js => js.LocationId)
				.HasDatabaseName("IX_LocationNotes_LocationId");

		}
	}
}
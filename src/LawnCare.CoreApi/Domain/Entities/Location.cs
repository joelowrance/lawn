using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.CoreApi.Domain.Entities
{
	public class Location : AggregateRoot
	{
		private List<LocationNote> _notes = new();

		public LocationId LocationId { get; private set; } = null!;
		public string Street1 { get; private set; } = null!;
		public string? Street2 { get; private set; }
		public string? Street3 { get; private set; }
		public string City { get; private set; } = null!;
		public string State { get; private set; } = null!;
		public Postcode Postcode { get; private set; }
		public Customer Owner { get; private set; } = null!;

		public IReadOnlyList<LocationNote> Notes => _notes.AsReadOnly();



		// EF Core constructor
		private Location()
		{
			Postcode = Postcode.Empty;
		}

		public Location(string street1, string? street2, string? street3, string city, string state, Postcode postcode,
			Customer owner)
		{
			if (string.IsNullOrWhiteSpace(street1) || string.IsNullOrWhiteSpace(city) ||
			    string.IsNullOrWhiteSpace(state))
			{
				throw new ArgumentException("Street1, City, and State are required");
			}

			LocationId = LocationId.Create();
			Street1 = street1;
			Street2 = street2;
			Street3 = street3;
			City = city;
			State = state;
			Postcode = postcode ?? throw new ArgumentNullException(nameof(postcode));
			Owner = owner ?? throw new ArgumentNullException(nameof(owner));
		}

		public void AddNote(string content, string userName)
		{
			_notes.Add(new LocationNote(LocationId, content, userName));
		}
	}

	public class LocationConfiguration : IEntityTypeConfiguration<Location>
	{
		public void Configure(EntityTypeBuilder<Location> builder)
		{
			// Table
			builder.ToTable("Locations");

			// Key
			builder.HasKey(l => l.LocationId);
			builder.Property(l => l.LocationId)
				.ValueGeneratedNever()
				.HasConversion(toDb => toDb.Value, fromDb => LocationId.From(fromDb));

			// Properties
			builder.Property(l => l.Street1)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(l => l.Street2)
				.HasMaxLength(200);

			builder.Property(l => l.Street3)
				.HasMaxLength(200);

			builder.Property(l => l.City)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(l => l.State)
				.IsRequired()
				.HasMaxLength(2);
			
			builder.Property(x => x.Postcode)
				.IsRequired()
				.HasColumnName("PostCode")
				.HasMaxLength(16)
				.HasConversion(toDb => toDb.Value, fromDb => new Postcode(fromDb))
				.Metadata.SetValueComparer(CoreDbContext.PostCodeComparer);

			// Relationship: Owner (many Locations to one Customer)
			builder.HasOne(l => l.Owner)
				.WithMany()
				.HasForeignKey("OwnerId")
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);

			// Backing field and relationship for notes
			builder.HasMany(l => l.Notes)
				.WithOne()
				.HasForeignKey(ln => ln.LocationId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			var notesNavigation = builder.Metadata.FindNavigation(nameof(Location.Notes));
			if (notesNavigation is not null)
			{
				notesNavigation.SetField("_notes");
				notesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
			}

			// Indexes
			builder.HasIndex("OwnerId");

		}
	}
		



}
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.JobApi.Infrastructure.Database
{
	internal class AddressConfiguration : IEntityTypeConfiguration<Address>
	{
		public void Configure(EntityTypeBuilder<Address> builder)
		{
			builder.ToTable("Address");
		
			builder.Property(x => x.Street1)
				.HasMaxLength(255)
				.IsRequired();
		
			builder.Property(x => x.Street2)
				.HasMaxLength(255)
				.IsRequired(false);
		
			builder.Property(x => x.Street3)
				.HasMaxLength(255)
				.IsRequired(false);

			builder.Property(x => x.City)
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(x => x.State)
				.HasMaxLength(2)
				.IsRequired();
		
			builder.Property(x => x.ZipCode)
				.HasMaxLength(15)
				.IsRequired();
		}
	}
}
using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.CoreApi.Domain.Entities
{
	public class Customer : Entity, IAuditable
	{
		public string FirstName { get; private set; } = null!;
		public string LastName { get; private set; } = null!;
		public EmailAddress Email { get; private set; } = null!;
		public PhoneNumber HomePhone { get; private set; } = null!;
		public PhoneNumber CellPhone { get; private set; } = null!;
		// public string FirstName { get; private set; } = null!;
		// public string FirstName { get; private set; } = null!;
		// public string FirstName { get; private set; } = null!;
		
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }

		public Customer()
		{
		}

		public Customer(string firstName, string lastName, EmailAddress email, PhoneNumber homePhone, PhoneNumber cellPhone)
		{
			FirstName = firstName;
			LastName = lastName;
			Email = email;
			HomePhone = homePhone;
			CellPhone = cellPhone;
		}
	}

	public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
	{
		public void Configure(EntityTypeBuilder<Customer> builder)
		{
			builder.ToTable("Customers", "public");
			builder.HasKey(x => x.Id);
			
			builder.Property(x => x.Id)
				.ValueGeneratedNever();
			
			builder.Property(x => x.FirstName)
				.IsRequired()
				.HasMaxLength(255)
				.HasComment("The first name of the customer");

			builder.Property(x => x.LastName)
				.IsRequired()
				.HasMaxLength(255);

			builder.Property(x => x.Email)
				.IsRequired()
				.HasMaxLength(255)
				.HasConversion<string>(
					toDb => toDb.Value,
					fromDb => new EmailAddress(fromDb))
				.Metadata.SetValueComparer(CoreDbContext.EmailComparer);
			
			
			builder.Property(x => x.HomePhone)
				.IsRequired()
				.HasMaxLength(255)
				.HasConversion<string>(
					toDb => toDb.Value,
					fromDb => new PhoneNumber(fromDb))
				.Metadata.SetValueComparer(CoreDbContext.PhoneNumberComparer);

			builder.Property(x => x.CellPhone)
				.IsRequired()
				.HasMaxLength(255)
				.HasConversion<string>(
					toDb => toDb.Value,
					fromDb => new PhoneNumber(fromDb))
				.Metadata.SetValueComparer(CoreDbContext.PhoneNumberComparer);
		}
	}
}
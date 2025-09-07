using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.CoreApi.Domain.Entities
{
	public class Technician : Entity, IAuditable
	{
		public string FirstName { get; private set; } = null!;
		public string LastName { get; private set; } = null!;
		public EmailAddress Email { get; private set; } = null!;
		public PhoneNumber CellPhone { get; private set; } = null!;
		public string Address { get; private set; } = null!;
		public string PhotoUrl { get; private set; } = string.Empty;
		public DateTime StartDate { get; private set; }
		public TechnicianStatus Status { get; private set; }
		public TechnicianSpecialization Specialization { get; private set; }
		public DateTime HireDate { get; private set; }
		public string EmergencyContact { get; private set; } = string.Empty;
		public PhoneNumber EmergencyPhone { get; private set; } = null!;
		public string LicenseNumber { get; private set; } = string.Empty;
		public DateTime LicenseExpiry { get; private set; }
		public string Notes { get; private set; } = string.Empty;

		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }

        // EF cpore
		private Technician()
		{
		}

		public Technician(
			string firstName,
			string lastName,
			EmailAddress email,
			PhoneNumber cellPhone,
			string address,
			TechnicianStatus status,
			TechnicianSpecialization specialization,
			DateTime startDate,
			PhoneNumber emergencyPhone,
			string licenseNumber,
			DateTime licenseExpiry)
		{
			FirstName = firstName;
			LastName = lastName;
			Email = email;
			CellPhone = cellPhone;
			Address = address;
			Status = status;
			Specialization = specialization;
			StartDate = startDate;
			EmergencyPhone = emergencyPhone;
			LicenseNumber = licenseNumber;
			LicenseExpiry = licenseExpiry;
		}

		// public void UpdatePersonalInfo(string firstName, string lastName, EmailAddress email, PhoneNumber cellPhone, string address)
		// {
		// 	FirstName = firstName;
		// 	LastName = lastName;
		// 	Email = email;
		// 	CellPhone = cellPhone;
		// 	Address = address;
		// }
		//
		// public void UpdateStatus(TechnicianStatus status)
		// {
		// 	Status = status;
		// }
		//
		// public void UpdateSpecialization(TechnicianSpecialization specialization)
		// {
		// 	Specialization = specialization;
		// }
		//
		// public void UpdateEmergencyContact(string contact, PhoneNumber phone)
		// {
		// 	EmergencyContact = contact;
		// 	EmergencyPhone = phone;
		// }
		//
		// public void UpdateLicense(string licenseNumber, DateTime expiry)
		// {
		// 	LicenseNumber = licenseNumber;
		// 	LicenseExpiry = expiry;
		// }
		//
		// public void UpdateNotes(string notes)
		// {
		// 	Notes = notes;
		// }
		//
		// public void UpdatePhoto(string photoUrl)
		// {
		// 	PhotoUrl = photoUrl;
		// }
		//
		// public void UpdateStartDate(DateTime startDate)
		// {
		// 	StartDate = startDate;
		// }

		public string GetFullName() => $"{FirstName} {LastName}";

		public int GetYearsWithCompany()
		{
			var today = DateTime.Today;
			var years = today.Year - StartDate.Year;
			if (today.Month < StartDate.Month || (today.Month == StartDate.Month && today.Day < StartDate.Day))
			{
				years--;
			}
			return Math.Max(0, years);
		}

		public int GetMonthsWithCompany()
		{
			var today = DateTime.Today;
			var months = (today.Year - StartDate.Year) * 12 + today.Month - StartDate.Month;
			if (today.Day < StartDate.Day)
			{
				months--;
			}
			return Math.Max(0, months);
		}

		public string GetExperienceDisplay()
		{
			var years = GetYearsWithCompany();
			var months = GetMonthsWithCompany();

			if (years > 0)
			{
				return $"{years} year{(years == 1 ? "" : "s")}";
			}
			else
			{
				return $"{months} month{(months == 1 ? "" : "s")}";
			}
		}
	}

	public enum TechnicianStatus
	{
		Active = 1,
		Inactive = 2,
		OnLeave = 3,
		Terminated = 4
	}

	public enum TechnicianSpecialization
	{
		LawnCare = 1,
		TreeService = 2,
		Landscaping = 3,
		SnowRemoval = 4,
		General = 5
	}

	public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
	{
		public void Configure(EntityTypeBuilder<Technician> builder)
		{
			builder.ToTable("Technicians", "public");
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.ValueGeneratedNever();

			builder.Property(x => x.FirstName)
				.IsRequired()
				.HasMaxLength(255)
				.HasComment("The first name of the technician");

			builder.Property(x => x.LastName)
				.IsRequired()
				.HasMaxLength(255)
				.HasComment("The last name of the technician");

			builder.Property(x => x.Email)
				.IsRequired()
				.HasMaxLength(255)
				.HasConversion<string>(
					toDb => toDb.Value,
					fromDb => new EmailAddress(fromDb))
				.Metadata.SetValueComparer(CoreDbContext.EmailComparer);

			builder.Property(x => x.CellPhone)
				.IsRequired()
				.HasMaxLength(255)
				.HasConversion<string>(
					toDb => toDb.Value,
					fromDb => new PhoneNumber(fromDb))
				.Metadata.SetValueComparer(CoreDbContext.PhoneNumberComparer);

			builder.Property(x => x.Address)
				.IsRequired()
				.HasMaxLength(500)
				.HasComment("The full address of the technician");

			builder.Property(x => x.PhotoUrl)
				.HasMaxLength(1000)
				.HasComment("URL to the technician's profile photo");

			builder.Property(x => x.StartDate)
				.IsRequired()
				.HasColumnType("date")
				.HasComment("Date when the technician started with the company");

			builder.Property(x => x.Status)
				.IsRequired()
				.HasConversion<int>()
				.HasComment("Current status of the technician");

			builder.Property(x => x.Specialization)
				.IsRequired()
				.HasConversion<int>()
				.HasComment("Primary specialization of the technician");

			builder.Property(x => x.HireDate)
				.IsRequired()
				.HasColumnType("date")
				.HasComment("Date when the technician was hired");

			builder.Property(x => x.EmergencyContact)
				.HasMaxLength(255)
				.HasComment("Name of emergency contact person");

			builder.Property(x => x.EmergencyPhone)
				.HasMaxLength(255)
				.HasConversion<string>(
					toDb => toDb.Value,
					fromDb => new PhoneNumber(fromDb))
				.Metadata.SetValueComparer(CoreDbContext.PhoneNumberComparer);

			builder.Property(x => x.LicenseNumber)
				.HasMaxLength(100)
				.HasComment("Professional license number");

			builder.Property(x => x.LicenseExpiry)
				.HasColumnType("date")
				.HasComment("Expiration date of the professional license");

			builder.Property(x => x.Notes)
				.HasMaxLength(2000)
				.HasComment("Additional notes about the technician");

			// Audit fields
			builder.Property(x => x.CreatedAt)
				.IsRequired()
				.HasComment("When the record was created");

			builder.Property(x => x.UpdatedAt)
				.IsRequired()
				.HasComment("When the record was last updated");

			// Indexes
			builder.HasIndex(x => x.Status);
			builder.HasIndex(x => x.Specialization);
			builder.HasIndex(x => x.Email).IsUnique();
			builder.HasIndex(x => x.LicenseNumber);
		}
	}
}

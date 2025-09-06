using LawnCare.CustomerApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.CustomerApi.Infrastructure.Database;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", schema: "CustomerService");

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .IsRequired();

        // Required string properties with length validation
        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Customer's first name");

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Customer's last name");

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("Customer's email address");

        // Optional phone numbers
        builder.Property(c => c.HomePhone)
            .IsRequired(false)
            .HasMaxLength(20)
            .HasComment("Customer's home phone number");

        builder.Property(c => c.CellPhone)
            .IsRequired(false)
            .HasMaxLength(20)
            .HasComment("Customer's cell phone number");

        // Address properties
        builder.Property(c => c.Address1)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("Primary address line");

        builder.Property(c => c.Address2)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("Secondary address line");

        builder.Property(c => c.Address3)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("Tertiary address line");

        builder.Property(c => c.City)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("City");

        builder.Property(c => c.State)
            .IsRequired()
            .HasMaxLength(10)
            .HasComment("State or province");

        builder.Property(c => c.ZipCode)
            .IsRequired()
            .HasMaxLength(10)
            .HasComment("Postal code");

        // Enum properties
        builder.Property(c => c.CustomerType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(CustomerType.Residential);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(CustomerStatus.Active);

        // Optional properties
        builder.Property(c => c.Notes)
            .IsRequired(false)
            .HasMaxLength(1000)
            .HasComment("Additional notes about the customer");

        // Audit properties
        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasComment("Timestamp when customer was created");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasComment("Timestamp when customer was last updated");

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("User who created the customer");

        builder.Property(c => c.UpdatedBy)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("User who last updated the customer");

        // Indexes for common query patterns
        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("IX_Customers_TenantId");

        builder.HasIndex(c => c.Email)
            .HasDatabaseName("IX_Customers_Email");

        builder.HasIndex(c => new { c.TenantId, c.Email })
            .HasDatabaseName("IX_Customers_TenantId_Email")
            .IsUnique();

        builder.HasIndex(c => new { c.TenantId, c.LastName, c.FirstName })
            .HasDatabaseName("IX_Customers_TenantId_Name");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Customers_Status");
    }
}

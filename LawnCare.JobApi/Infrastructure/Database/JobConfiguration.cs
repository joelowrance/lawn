using System.Text.Json;

using JobService.Infrastructure.Persistence;

using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LawnCare.JobApi.Infrastructure.Database;

internal class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
	    
	    var customerIdConverter = new ValueConverter<CustomerId?, Guid?>(
		    v => v != null ? v.Value : null,
		    v =>  v.HasValue ? CustomerId.From(v.Value) : null);
	    
	    var technicianIdConverter = new ValueConverter<TechnicianId?, Guid?>(
		    v => v != null ? v.Value : null,
		    v => v.HasValue ? TechnicianId.From(v.Value) : null);

	    // Money conversion - store as JSON
	    var moneyConverter = new ValueConverter<Money, string>(
		    v => JsonSerializer.Serialize(new MoneyDto(v.Amount, v.Currency), JobDbContext.JsonOptions),
		    v => DeserializeMoney(v));

	    // Nullable Money conversion
	    var nullableMoneyConverter = new ValueConverter<Money?, string?>(
		    v => v != null ? JsonSerializer.Serialize(new MoneyDto(v.Amount, v.Currency), JobDbContext.JsonOptions) : null,
		    v => DeserializeNullableMoney(v));

	    // EstimatedDuration conversion - store as TimeSpan ticks
	    var estimatedDurationConverter = new ValueConverter<EstimatedDuration, long>(
		    v => v.Duration.Ticks,
		    v => new EstimatedDuration((int)v));
	    
	    var serviceAddressConverter = new ValueConverter<ServiceAddress, string>(
		    v => JsonSerializer.Serialize(new ServiceAddressDto(
			    v.Street1, v.Street2, v.Street3, v.City, v.State, v.ZipCode, v.Latitude, v.Longitude), JobDbContext.JsonOptions),
		    v => DeserializeServiceAddress(v));
	    
        builder.ToTable("Jobs", schema: "JobService")
	        .ToTable(t => t.HasCheckConstraint("CK_Jobs_ScheduledDate_Future",
		        "\"scheduled_date\" IS NULL OR \"scheduled_date\" >= \"created_at\""))
	        .ToTable(t => t.HasCheckConstraint("CK_Jobs_CompletedDate_Future",
		        "\"completed_date\" IS NULL OR \"completed_date\" >= \"created_at\""));


        
        // Primary key
        builder.HasKey(j => j.JobId);
        builder.Property(j => j.JobId)
        	        .HasColumnName("id")
	        .ValueGeneratedNever() // We generate IDs in domain
	        .HasConversion(
		        v => v.Value,
		        v => JobId.From(v));
        
        builder.Property(e => e.TenantId)
	        .HasConversion(
		        v => v.Value,
		        v => TenantId.From(v))
	        .IsRequired();
        
        builder.Property(e => e.CustomerId)
	        .HasConversion(customerIdConverter);

        builder.Property(e => e.AssignedTechnicianId)
	        .HasConversion(technicianIdConverter);

        // Required string properties
        builder.Property(j => j.CustomerName)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Name of the customer for this job");

        builder.Property(j => j.Description)
            .IsRequired()
            .HasMaxLength(2000)
            .HasComment("Detailed description of the work to be performed");
        
        builder.Property(e => e.ActualCost)
	        .HasConversion(nullableMoneyConverter)
	        .HasColumnType("text");

        builder.Property(e => e.EstimatedDuration)
	        .HasConversion(estimatedDurationConverter)
	        .IsRequired();
        
        builder.Property(e => e.EstimatedCost)
	        .HasConversion(moneyConverter)
	        .HasColumnType("text")
	        .IsRequired();
        
        builder.Property(e => e.ServiceAddress)
	        .HasConversion(serviceAddressConverter)
	        .HasColumnType("text")
	        .IsRequired();

        // Enum conversions
        builder.Property(e => e.Status)
	        .HasConversion<string>()
	        .HasMaxLength(50)
	        .IsRequired();

        builder.Property(e => e.Priority)
	        .HasConversion<string>()
	        .HasMaxLength(50)
	        .IsRequired();

        // Optional string properties
        builder.Property(j => j.SpecialInstructions)
            .IsRequired(false)
            .HasMaxLength(1000)
            .HasComment("Special instructions from customer or management");

        // DateTime properties
        builder.Property(j => j.RequestedDate)
            .IsRequired()
            .HasComment("Date requested by customer");

        builder.Property(j => j.ScheduledDate)
            .IsRequired(false)
            .HasComment("Date scheduled for technician");

        builder.Property(j => j.CompletedDate)
            .IsRequired(false)
            .HasComment("Date job was completed");

        builder.Property(j => j.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasComment("Timestamp when job was created");

        builder.Property(j => j.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasComment("Timestamp when job was last updated");

        // Configure child entity relationships
        builder.HasMany(j => j.ServiceItems)
            .WithOne()
            .HasForeignKey("JobId")
            .HasPrincipalKey(j => j.JobId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_JobRequirements_Job");

        builder.HasMany(j => j.Notes)
            .WithOne()
            .HasForeignKey("JobId")
            .HasPrincipalKey(j => j.JobId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_JobNotes_Job");

        // Configure backing fields for collections
        builder.Navigation(j => j.ServiceItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_services");

        builder.Navigation(j => j.Notes)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_notes");
        
        

        // Performance indexes
        builder.HasIndex(j => j.TenantId)
            .HasDatabaseName("IX_Jobs_TenantId");

        builder.HasIndex(j => j.CustomerId)
            .HasDatabaseName("IX_Jobs_CustomerId");

        builder.HasIndex(j => j.AssignedTechnicianId)
            .HasDatabaseName("IX_Jobs_AssignedTechnicianId");

        builder.HasIndex(j => j.Status)
            .HasDatabaseName("IX_Jobs_Status");

        builder.HasIndex(j => j.Priority)
            .HasDatabaseName("IX_Jobs_Priority");

        builder.HasIndex(j => j.RequestedDate)
            .HasDatabaseName("IX_Jobs_RequestedDate");

        builder.HasIndex(j => j.ScheduledDate)
            .HasDatabaseName("IX_Jobs_ScheduledDate");

        builder.HasIndex(j => j.CreatedAt)
            .HasDatabaseName("IX_Jobs_CreatedAt");

        // Composite indexes for common query patterns
        builder.HasIndex(j => new { j.TenantId, j.Status })
            .HasDatabaseName("IX_Jobs_TenantId_Status");

        builder.HasIndex(j => new { j.TenantId, j.CustomerId })
            .HasDatabaseName("IX_Jobs_TenantId_CustomerId");

        builder.HasIndex(j => new { j.TenantId, j.AssignedTechnicianId, j.ScheduledDate })
            .HasDatabaseName("IX_Jobs_TenantId_TechnicianId_ScheduledDate");

        // Ignore domain events (not persisted)
        builder.Ignore(j => j.DomainEvents);

        // Table constraints
        //builder.HasCheckConstraint("CK_Jobs_ScheduledDate_Future", "[ScheduledDate] IS NULL OR [ScheduledDate] >= [CreatedAt]");
        //builder.HasCheckConstraint("CK_Jobs_CompletedDate_Future", "[CompletedDate] IS NULL OR [CompletedDate] >= [CreatedAt]");
    }
    
    private static Money DeserializeMoney(string json)
    {
	    // Handle null/empty JSON
	    if (string.IsNullOrWhiteSpace(json))
	    {
		    return Money.Zero(); // Safe fallback
	    }

	    try
	    {
		    var dto = JsonSerializer.Deserialize<MoneyDto>(json);
		    return dto != null 
			    ? new Money(dto.Amount, dto.Currency)
			    : Money.Zero(); // Safe fallback if deserialization returns null
	    }
	    catch (JsonException)
	    {
		    // If JSON is corrupted, return safe fallback instead of throwing
		    // This prevents the entire query from failing due to bad data
		    return Money.Zero();
	    }
    }
        
    private static Money? DeserializeNullableMoney(string? json)
    {
	    if (string.IsNullOrWhiteSpace(json))
	    {
		    return null; // Explicitly null
	    }

	    try
	    {
		    var dto = JsonSerializer.Deserialize<MoneyDto>(json, JobDbContext.JsonOptions);
		    return dto != null ? new Money(dto.Amount, dto.Currency) : null;
	    }
	    catch (JsonException)
	    {
		    return null; // Safe fallback for corrupted data
	    }
    }
    
    private static ServiceAddress DeserializeServiceAddress(string json)
    {
	    if (string.IsNullOrWhiteSpace(json))
	    {
		    return CreateDefaultAddress(); // Default fallback
	    }
        
	    try
	    {
		    var dto = JsonSerializer.Deserialize<ServiceAddressDto>(json, JobDbContext.JsonOptions);
		    return dto != null 
			    ? new ServiceAddress(dto.Street1, dto.Street2, dto.Street3, dto.City, dto.State, dto.ZipCode, 
				    dto.Latitude, dto.Longitude)
			    : CreateDefaultAddress(); // Safe fallback
	    }
	    catch (JsonException)
	    {
		    return CreateDefaultAddress(); // Safe fallback for corrupted data
	    }
    }

    // private static EstimatedDuration CreateEstimatedDurationFromTicks(long ticks)
    // {
    //     var timeSpan = TimeSpan.FromTicks(ticks);
    //     return new EstimatedDuration(
    //         (int)timeSpan.TotalHours, 
    //         timeSpan.Minutes % 60);
    // }
    //
    private static ServiceAddress CreateDefaultAddress()
    {
	    return new ServiceAddress("Unknown", "Unknown", "Unknown", "Unknown", "IL", "00000");
    }
}
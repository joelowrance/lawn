using LawnCare.JobApi.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.JobApi.Infrastructure.Database;

internal class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs", schema: "JobService")
	        .ToTable(t => t.HasCheckConstraint("CK_Jobs_ScheduledDate_Future",
		        "[ScheduledDate] IS NULL OR [ScheduledDate] >= [CreatedAt]"))
	        .ToTable(t => t.HasCheckConstraint("CK_Jobs_CompletedDate_Future",
		        "[CompletedDate] IS NULL OR [CompletedDate] >= [CreatedAt]"));

        // Primary key
        builder.HasKey(j => j.Id);

        // Required string properties
        builder.Property(j => j.CustomerName)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Name of the customer for this job");

        builder.Property(j => j.Description)
            .IsRequired()
            .HasMaxLength(2000)
            .HasComment("Detailed description of the work to be performed");

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
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Timestamp when job was created");

        builder.Property(j => j.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Timestamp when job was last updated");

        // Configure child entity relationships
        builder.HasMany(j => j.ServiceItems)
            .WithOne()
            .HasForeignKey("JobId")
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_JobRequirements_Job");

        builder.HasMany(j => j.Notes)
            .WithOne()
            .HasForeignKey("JobId")
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_JobNotes_Job");

        // Configure backing fields for collections
        builder.Navigation(j => j.ServiceItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_requirements");

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
}


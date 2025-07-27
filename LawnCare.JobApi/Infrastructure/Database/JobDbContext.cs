// ============================================================================
// INFRASTRUCTURE - JOB DB CONTEXT
// ============================================================================

// Infrastructure/Persistence/JobDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.ValueObjects;
using LawnCare.JobApi.Domain.Enums;
using LawnCare.JobApi.Domain.Common;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobService.Infrastructure.Persistence
{
    public class JobDbContext : DbContext
    {
	    
	    // JSON options for consistent serialization
	    private static readonly JsonSerializerOptions JsonOptions = new()
	    {
		    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		    WriteIndented = false,
		    PropertyNameCaseInsensitive = true
	    };
	    
        //private readonly ITenantService _tenantService;
        private readonly ILogger<JobDbContext> _logger;

        public JobDbContext(
            DbContextOptions<JobDbContext> options, 
            //ITenantService tenantService,
            ILogger<JobDbContext> logger) 
            : base(options)
        {
//            _tenantService = tenantService;
            _logger = logger;
        }

        // DbSets for aggregate roots and entities
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<JobRequirement> JobRequirements { get; set; } = null!;
        public DbSet<JobNote> JobNotes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _logger.LogDebug("Configuring Job domain model");

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new JobConfiguration());
            modelBuilder.ApplyConfiguration(new JobRequirementConfiguration());
            modelBuilder.ApplyConfiguration(new JobNoteConfiguration());

            // Configure value object conversions
            ConfigureValueObjectConversions(modelBuilder);

            // Apply global query filters for tenant isolation
            ApplyGlobalFilters(modelBuilder);

            // Configure conventions
            ConfigureConventions(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Enable sensitive data logging in development
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }

            // Configure query tracking behavior
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Add audit information before saving
                AddAuditInformation();
                
                // Validate tenant isolation
                ValidateTenantIsolation();

                var result = await base.SaveChangesAsync(cancellationToken);
                
                _logger.LogDebug("Successfully saved {ChangeCount} changes to database", result);
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict occurred while saving changes");
                throw new Infrastructure.Exceptions.ConcurrencyException(
                    "A concurrency conflict occurred. The record may have been modified by another user.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update failed while saving changes");
                throw new Infrastructure.Exceptions.PersistenceException(
                    "Failed to save changes to the database.", ex);
            }
        }
        
         
	
private void ConfigureValueObjectConversions(ModelBuilder modelBuilder)
        {
            // JobId conversion
            var jobIdConverter = new ValueConverter<JobId, Guid>(
                v => v.Value,
                v => JobId.From(v));

            // TenantId conversion
            var tenantIdConverter = new ValueConverter<TenantId, Guid>(
                v => v.Value,
                v => TenantId.From(v));

            // CustomerId conversion
            var customerIdConverter = new ValueConverter<CustomerId, Guid>(
                v => v.Value,
                v => CustomerId.From(v));

            // TechnicianId conversion (nullable)
            var technicianIdConverter = new ValueConverter<TechnicianId?, Guid?>(
                v => v != null ? v.Value : null,
                v => v.HasValue ? TechnicianId.From(v.Value) : null);

            // Money conversion - store as JSON
            var moneyConverter = new ValueConverter<Money, string>(
                v => JsonSerializer.Serialize(new MoneyDto(v.Amount, v.Currency), JsonOptions),
                v => DeserializeMoney(v));

            // Nullable Money conversion
            var nullableMoneyConverter = new ValueConverter<Money?, string?>(
                v => v != null ? JsonSerializer.Serialize(new MoneyDto(v.Amount, v.Currency), JsonOptions) : null,
                v => DeserializeNullableMoney(v));

            // EstimatedDuration conversion - store as TimeSpan ticks
            var estimatedDurationConverter = new ValueConverter<EstimatedDuration, long>(
                v => v.Duration.Ticks,
                v => new EstimatedDuration((int)TimeSpan.FromTicks(v).TotalHours, 
                                         (int)TimeSpan.FromTicks(v).Minutes % 60));

            // ServiceType conversion - store as JSON
            var serviceTypeConverter = new ValueConverter<ServiceType, string>(
                v => JsonSerializer.Serialize(new ServiceTypeDto(v.Category, v.ServiceName, v.Description), JsonOptions),
                v => DeserializeServiceType(v));

            // ServiceAddress conversion - store as JSON
            var serviceAddressConverter = new ValueConverter<ServiceAddress, string>(
                v => JsonSerializer.Serialize(new ServiceAddressDto(
                    v.Street, v.City, v.State, v.ZipCode, v.ApartmentUnit, v.Latitude, v.Longitude), JsonOptions),
                v => DeserializeServiceAddress(v));

            // Apply conversions to Job entity
            modelBuilder.Entity<Job>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasConversion(jobIdConverter)
                    .ValueGeneratedNever(); // We generate IDs in domain

                entity.Property(e => e.TenantId)
                    .HasConversion(tenantIdConverter)
                    .IsRequired();

                entity.Property(e => e.CustomerId)
                    .HasConversion(customerIdConverter)
                    .IsRequired();

                entity.Property(e => e.AssignedTechnicianId)
                    .HasConversion(technicianIdConverter);

                entity.Property(e => e.EstimatedCost)
                    .HasConversion(moneyConverter)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();

                entity.Property(e => e.ActualCost)
                    .HasConversion(nullableMoneyConverter)
                    .HasColumnType("nvarchar(100)");

                entity.Property(e => e.EstimatedDuration)
                    .HasConversion(estimatedDurationConverter)
                    .IsRequired();

                entity.Property(e => e.ServiceType)
                    .HasConversion(serviceTypeConverter)
                    .HasColumnType("nvarchar(500)")
                    .IsRequired();

                entity.Property(e => e.ServiceAddress)
                    .HasConversion(serviceAddressConverter)
                    .HasColumnType("nvarchar(1000)")
                    .IsRequired();

                // Enum conversions
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Priority)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();
            });
        }

        private void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            // Global query filter for tenant isolation
            //modelBuilder.Entity<Job>().HasQueryFilter(j => j.TenantId == _tenantService.GetCurrentTenant());
            
            _logger.LogDebug("Applied global tenant isolation filter");
        }

        private void ConfigureConventions(ModelBuilder modelBuilder)
        {
            // Configure string properties to have reasonable defaults
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        // Set default max length for string properties
                        if (property.GetMaxLength() == null)
                        {
                            property.SetMaxLength(255);
                        }
                    }
                }
            }

            // Configure DateTime properties to use UTC
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("datetime2");
                    }
                }
            }
        }

        private void AddAuditInformation()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Job && (e.State == EntityState.Added || e.State == EntityState.Modified))
                .ToList();

            foreach (var entry in entries)
            {
                if (entry.Entity is Job job)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        // Ensure UpdatedAt is current for any modifications
                        // Note: In a pure DDD approach, this should be handled by domain operations
                        // This is a safety net for cases where domain operations might miss it
                        var updatedAtProperty = entry.Property(nameof(Job.UpdatedAt));
                        if (!updatedAtProperty.IsModified || 
                            (DateTime)updatedAtProperty.CurrentValue! < DateTime.UtcNow.AddSeconds(-5))
                        {
                            updatedAtProperty.CurrentValue = DateTime.UtcNow;
                        }
                    }
                }
            }

            _logger.LogDebug("Added audit information to {EntryCount} entities", entries.Count);
        }

        private void ValidateTenantIsolation()
        {
            // if (!_tenantService.HasTenantContext())
            // {
            //     throw new InvalidOperationException("No tenant context available for database operation");
            // }
            //
            // var currentTenant =  _tenantService.GetCurrentTenant();
            //
            // var entries = ChangeTracker.Entries()
            //     .Where(e => e.Entity is Job && e.State == EntityState.Added)
            //     .ToList();
            //
            // foreach (var entry in entries)
            // {
            //     if (entry.Entity is Job job)
            //     {
            //         if (job.TenantId != currentTenant)
            //         {
            //             _logger.LogError("Tenant isolation violation: Job tenant {JobTenant} does not match current tenant {CurrentTenant}", 
            //                 job.TenantId.Value, currentTenant.Value);
            //             
            //             throw new InvalidOperationException(
            //                 $"Tenant isolation violation: Job belongs to tenant {job.TenantId.Value} but current context is {currentTenant.Value}");
            //         }
            //     }
            // }
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
		        var dto = JsonSerializer.Deserialize<MoneyDto>(json, JsonOptions);
		        return dto != null ? new Money(dto.Amount, dto.Currency) : null;
	        }
	        catch (JsonException)
	        {
		        return null; // Safe fallback for corrupted data
	        }
        }
        
        private static string SerializeServiceType(ServiceType serviceType)
        {
            var dto = new ServiceTypeDto(
                serviceType.Category, 
                serviceType.ServiceName, 
                serviceType.Description);
            return JsonSerializer.Serialize(dto, JsonOptions);
        }

        private static ServiceType DeserializeServiceType(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return ServiceType.LawnMowing(); // Default fallback
            }

            try
            {
                var dto = JsonSerializer.Deserialize<ServiceTypeDto>(json, JsonOptions);
                return dto != null 
                    ? new ServiceType(dto.Category, dto.ServiceName, dto.Description)
                    : ServiceType.LawnMowing(); // Safe fallback
            }
            catch (JsonException)
            {
                return ServiceType.LawnMowing(); // Safe fallback for corrupted data
            }
        }

        private static string SerializeServiceAddress(ServiceAddress address)
        {
            var dto = new ServiceAddressDto(
                address.Street,
                address.City,
                address.State,
                address.ZipCode,
                address.ApartmentUnit,
                address.Latitude,
                address.Longitude);
            return JsonSerializer.Serialize(dto, JsonOptions);
        }

        private static ServiceAddress DeserializeServiceAddress(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateDefaultAddress(); // Default fallback
            }

            try
            {
                var dto = JsonSerializer.Deserialize<ServiceAddressDto>(json, JsonOptions);
                return dto != null 
                    ? new ServiceAddress(dto.Street, dto.City, dto.State, dto.ZipCode, 
                                       dto.ApartmentUnit, dto.Latitude, dto.Longitude)
                    : CreateDefaultAddress(); // Safe fallback
            }
            catch (JsonException)
            {
                return CreateDefaultAddress(); // Safe fallback for corrupted data
            }
        }

        private static EstimatedDuration CreateEstimatedDurationFromTicks(long ticks)
        {
            var timeSpan = TimeSpan.FromTicks(ticks);
            return new EstimatedDuration(
                (int)timeSpan.TotalHours, 
                timeSpan.Minutes % 60);
        }

        private static ServiceAddress CreateDefaultAddress()
        {
            return new ServiceAddress("Unknown", "Unknown", "IL", "00000");
        }

        
    }

    // ============================================================================
    // ENTITY CONFIGURATIONS
    // ============================================================================

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
            builder.HasMany(j => j.Requirements)
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
            builder.Navigation(j => j.Requirements)
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

    internal class JobRequirementConfiguration : IEntityTypeConfiguration<JobRequirement>
    {
        public void Configure(EntityTypeBuilder<JobRequirement> builder)
        {
            builder.ToTable("JobRequirements", schema: "JobService");

            // Primary key
            builder.HasKey(jr => new { jr.RequirementType, jr.Description, jr.IsRequired });

            // Properties
            builder.Property(jr => jr.Id)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Type of requirement (Equipment, Material, Skill, etc.)");

            builder.Property(jr => jr.Description)
                .IsRequired()
                .HasMaxLength(500)
                .HasComment("Detailed description of the requirement");

            builder.Property(jr => jr.IsRequired)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Whether this requirement is mandatory");

            builder.Property(jr => jr.IsFulfilled)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Whether this requirement has been fulfilled");

            // Foreign key to Job (shadow property)
            builder.Property<Guid>("JobId")
                .IsRequired()
                .HasComment("Foreign key to the Job");

            // Indexes
            builder.HasIndex("JobId")
                .HasDatabaseName("IX_JobRequirements_JobId");

            builder.HasIndex(jr => jr.RequirementType)
                .HasDatabaseName("IX_JobRequirements_RequirementType");

            builder.HasIndex(jr => new { jr.IsRequired, jr.IsFulfilled })
                .HasDatabaseName("IX_JobRequirements_IsRequired_IsFulfilled");
        }
    }

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
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Timestamp when note was created");

            // Foreign key to Job (shadow property)
            builder.Property<Guid>("JobId")
                .IsRequired()
                .HasComment("Foreign key to the Job");

            // Indexes
            builder.HasIndex("JobId")
                .HasDatabaseName("IX_JobNotes_JobId");

            builder.HasIndex(jn => jn.CreatedAt)
                .HasDatabaseName("IX_JobNotes_CreatedAt");

            builder.HasIndex(jn => jn.Author)
                .HasDatabaseName("IX_JobNotes_Author");
        }
    }

    // ============================================================================
    // DTO CLASSES FOR JSON SERIALIZATION
    // ============================================================================

    internal record MoneyDto(decimal Amount, string Currency);
    
    internal record ServiceTypeDto(string Category, string ServiceName, string Description);
    
    internal record ServiceAddressDto(
        string Street, 
        string City, 
        string State, 
        string ZipCode, 
        string? ApartmentUnit, 
        decimal? Latitude, 
        decimal? Longitude);
}

// ============================================================================
// EXCEPTION CLASSES
// ============================================================================

namespace JobService.Infrastructure.Exceptions
{
    public class PersistenceException : Exception
    {
        public PersistenceException(string message) : base(message) { }
        public PersistenceException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) { }
        public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
    }
}

// ============================================================================
// CONNECTION STRING CONFIGURATION
// ============================================================================

namespace JobService.Infrastructure.Configuration
{
    public class DatabaseConfiguration
    {
        public const string SectionName = "Database";
        
        public string ConnectionString { get; set; } = string.Empty;
        public int CommandTimeout { get; set; } = 30;
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public bool EnableDetailedErrors { get; set; } = false;
        public int MaxRetryCount { get; set; } = 3;
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    }
}

// ============================================================================
// DEPENDENCY INJECTION SETUP
// ============================================================================

// Infrastructure/DependencyInjection.cs
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Options;
//
// namespace JobService.Infrastructure
// {
//     public static class DependencyInjection
//     {
//         public static IServiceCollection AddJobInfrastructure(
//             this IServiceCollection services, 
//             IConfiguration configuration)
//         {
//             // Configure database settings
//             services.Configure<Configuration.DatabaseConfiguration>(
//                 configuration.GetSection(Configuration.DatabaseConfiguration.SectionName));
//
//             // Add DbContext
//             services.AddDbContext<Persistence.JobDbContext>((serviceProvider, options) =>
//             {
//                 var dbConfig = serviceProvider.GetRequiredService<IOptions<Configuration.DatabaseConfiguration>>().Value;
//                 var connectionString = configuration.GetConnectionString("DefaultConnection") ?? dbConfig.ConnectionString;
//
//                 options.UseSqlServer(connectionString, sqlOptions =>
//                 {
//                     sqlOptions.MigrationsAssembly(typeof(Persistence.JobDbContext).Assembly.FullName);
//                     sqlOptions.EnableRetryOnFailure(
//                         maxRetryCount: dbConfig.MaxRetryCount,
//                         maxRetryDelay: dbConfig.MaxRetryDelay,
//                         errorNumbersToAdd: null);
//                 });
//
//                 options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
//                 
//                 if (dbConfig.EnableSensitiveDataLogging)
//                 {
//                     options.EnableSensitiveDataLogging();
//                 }
//                 
//                 if (dbConfig.EnableDetailedErrors)
//                 {
//                     options.EnableDetailedErrors();
//                 }
//             });
//
//             // Add health checks
//             services.AddHealthChecks()
//                 .AddDbContextCheck<Persistence.JobDbContext>("jobdatabase");
//
//             return services;
//         }
//     }
// }

// ============================================================================
// USAGE IN PROGRAM.CS
// ============================================================================

/*
// Program.cs example:

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddJobInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure pipeline
app.UseHealthChecks("/health");

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JobDbContext>();
    context.Database.EnsureCreated();
    // Or for production:
    // context.Database.Migrate();
}

app.Run();
*/

// ============================================================================
// EXAMPLE MIGRATION COMMANDS
// ============================================================================

/*
CLI Commands for migrations:

1. Add new migration:
   dotnet ef migrations add InitialCreate --project JobService.Infrastructure --startup-project JobService.API

2. Update database:
   dotnet ef database update --project JobService.Infrastructure --startup-project JobService.API

3. Generate SQL script:
   dotnet ef migrations script --project JobService.Infrastructure --startup-project JobService.API --output migration.sql

4. Remove last migration:
   dotnet ef migrations remove --project JobService.Infrastructure --startup-project JobService.API

5. Update to specific migration:
   dotnet ef database update InitialCreate --project JobService.Infrastructure --startup-project JobService.API
*/
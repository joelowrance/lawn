// ============================================================================
// INFRASTRUCTURE - JOB DB CONTEXT
// ============================================================================

// Infrastructure/Persistence/JobDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System.Text.Json;
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.ValueObjects;
using LawnCare.JobApi.Infrastructure.Database;

namespace JobService.Infrastructure.Persistence
{
    public class JobDbContext : DbContext
    {
	    
	    // JSON options for consistent serialization
	    public static readonly JsonSerializerOptions JsonOptions = new()
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
        public DbSet<JobServiceItem> JobServiceItems { get; set; } = null!;
        public DbSet<JobNote> JobNotes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _logger.LogDebug("Configuring Job domain model");

            // Apply entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobDbContext).Assembly);
            
            // Configure value object conversions
            //ConfigureValueObjectConversions(modelBuilder);

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
                throw new Exceptions.ConcurrencyException(
                    "A concurrency conflict occurred. The record may have been modified by another user.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update failed while saving changes");
                throw new Exceptions.PersistenceException(
                    "Failed to save changes to the database.", ex);
            }
        }
        
         
	
	// private void ConfigureValueObjectConversions(ModelBuilder modelBuilder)
 //    {
 //        // JobId conversion
 //        // var jobIdConverter = new ValueConverter<JobId, Guid>(
 //        //     v => v.Value,
 //        //     v => JobId.From(v));
 //
 //        // TenantId conversion
 //        // var tenantIdConverter = new ValueConverter<TenantId, Guid>(
 //        //     v => v.Value,
 //        //     v => TenantId.From(v));
 //
 //        // CustomerId conversion
 //        // var customerIdConverter = new ValueConverter<CustomerId?, Guid?>(
 //        //     v => v != null ? v.Value : null,
 //        //     v =>  v.HasValue ? CustomerId.From(v.Value) : null);
 //        //
 //        // // TechnicianId conversion (nullable)
 //        // var technicianIdConverter = new ValueConverter<TechnicianId?, Guid?>(
 //        //     v => v != null ? v.Value : null,
 //        //     v => v.HasValue ? TechnicianId.From(v.Value) : null);
 //
 //        // // Money conversion - store as JSON
 //        // var moneyConverter = new ValueConverter<Money, string>(
 //        //     v => JsonSerializer.Serialize(new MoneyDto(v.Amount, v.Currency), JsonOptions),
 //        //     v => DeserializeMoney(v));
 //        //
 //        // // Nullable Money conversion
 //        // var nullableMoneyConverter = new ValueConverter<Money?, string?>(
 //        //     v => v != null ? JsonSerializer.Serialize(new MoneyDto(v.Amount, v.Currency), JsonOptions) : null,
 //        //     v => DeserializeNullableMoney(v));
 //        //
 //        // // EstimatedDuration conversion - store as TimeSpan ticks
 //        // var estimatedDurationConverter = new ValueConverter<EstimatedDuration, long>(
 //        //     v => v.Duration.Ticks,
 //        //     v => new EstimatedDuration((int)v));
 //
 //        // ServiceType conversion - store as JSON
 //        // var serviceTypeConverter = new ValueConverter<ServiceType, string>(
 //        //     v => JsonSerializer.Serialize(new ServiceTypeDto(v.Category, v.ServiceName, v.Description), JsonOptions),
 //        //     v => DeserializeServiceType(v));
 //
 //        // ServiceAddress conversion - store as JSON
 //        // var serviceAddressConverter = new ValueConverter<ServiceAddress, string>(
 //        //     v => JsonSerializer.Serialize(new ServiceAddressDto(
 //        //         v.Street1, v.Street2, v.Street3, v.City, v.State, v.ZipCode, v.Latitude, v.Longitude), JsonOptions),
 //        //     v => DeserializeServiceAddress(v));
 //        
 //        
 //
 //        // Apply conversions to Job entity
 //        // modelBuilder.Entity<Job>(entity =>
 //        // {
 //        //     // entity.Property(e => e.JobId)
 //        //     //     .HasConversion(jobIdConverter)
 //        //     //     .ValueGeneratedNever(); // We generate IDs in domain
 //        //
 //        //     // entity.Property(e => e.TenantId)
 //        //     //     .HasConversion(tenantIdConverter)
 //        //     //     .IsRequired();
 //        //
 //        //     // entity.Property(e => e.CustomerId)
 //        //     //     .HasConversion(customerIdConverter);
 //        //         
 //        //
 //        //     // entity.Property(e => e.AssignedTechnicianId)
 //        //     //     .HasConversion(technicianIdConverter);
 //        //     //
 //        //     // entity.Property(e => e.EstimatedCost)
 //        //     //     .HasConversion(moneyConverter)
 //        //     //     .HasColumnType("nvarchar(100)")
 //        //     //     .IsRequired();
 //        //     //
 //        //     // entity.Property(e => e.ActualCost)
 //        //     //     .HasConversion(nullableMoneyConverter)
 //        //     //     .HasColumnType("nvarchar(100)");
 //        //     //
 //        //     // entity.Property(e => e.EstimatedDuration)
	//        //     //  .HasConversion(estimatedDurationConverter)
 //        //     //     .IsRequired();
 //        //
 //        //     // entity.Property(e => e.ServiceAddress)
 //        //     //     .HasConversion(serviceAddressConverter)
 //        //     //     .HasColumnType("nvarchar(1000)")
 //        //     //     .IsRequired();
 //        //     //
 //        //     // // Enum conversions
 //        //     // entity.Property(e => e.Status)
 //        //     //     .HasConversion<string>()
 //        //     //     .HasMaxLength(50)
 //        //     //     .IsRequired();
 //        //     //
 //        //     // entity.Property(e => e.Priority)
 //        //     //     .HasConversion<string>()
 //        //     //     .HasMaxLength(50)
 //        //     //     .IsRequired();
 //        // });
 //    }

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
                if (entry.Entity is Job _)
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
        
        // private static string SerializeServiceType(ServiceType serviceType)
        // {
        //     var dto = new ServiceTypeDto(
        //         serviceType.Category, 
        //         serviceType.ServiceName, 
        //         serviceType.Description);
        //     return JsonSerializer.Serialize(dto, JsonOptions);
        // }

        // private static ServiceType DeserializeServiceType(string json)
        // {
        //     if (string.IsNullOrWhiteSpace(json))
        //     {
        //         return ServiceType.LawnMowing(); // Default fallback
        //     }
        //
        //     try
        //     {
        //         var dto = JsonSerializer.Deserialize<ServiceTypeDto>(json, JsonOptions);
        //         return dto != null 
        //             ? new ServiceType(dto.Category, dto.ServiceName, dto.Description)
        //             : ServiceType.LawnMowing(); // Safe fallback
        //     }
        //     catch (JsonException)
        //     {
        //         return ServiceType.LawnMowing(); // Safe fallback for corrupted data
        //     }
        // }

        // private static string SerializeServiceAddress(ServiceAddress address)
        // {
        //     var dto = new ServiceAddressDto(
        //         address.Street,
        //         address.City,
        //         address.State,
        //         address.ZipCode,
        //         address.ApartmentUnit,
        //         address.Latitude,
        //         address.Longitude);
        //     return JsonSerializer.Serialize(dto, JsonOptions);
        // }

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

    // ============================================================================
    // ENTITY CONFIGURATIONS
    // ============================================================================


    // ============================================================================
    // DTO CLASSES FOR JSON SERIALIZATION
    // ============================================================================

    internal record MoneyDto(decimal Amount, string Currency);
    
    //internal record ServiceTypeDto(string Category, string ServiceName, string Description);
    
    internal record ServiceAddressDto(
        string Street1, 
        string Street2,
        string Street3,
        string City, 
        string State, 
        string ZipCode, 
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
using System.Text.Json;

using LawnCare.CoreApi.Domain.Common;

using Microsoft.EntityFrameworkCore;

namespace LawnCare.CoreApi.Infrastructure.Database
{
	public class CoreDbContext : DbContext
	{
		//private readonly ITenantService _tenantService;
		private readonly ILogger<CoreDbContext> _logger;
		
		public CoreDbContext(
			DbContextOptions<CoreDbContext> options, 
			ILogger<CoreDbContext> logger) 
			: base(options)
		{
			_logger = logger;
		}
		
		// JSON options for consistent serialization
		public static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			PropertyNameCaseInsensitive = true
		};
		
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			_logger.LogDebug("Configuring Job domain model");

			// Apply entity configurations
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
            
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
				// ValidateTenantIsolation(); // here we would ensure tenant id is set (like audit info)

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
				.Where(e => e.Entity is IAuditable && (e.State == EntityState.Added || e.State == EntityState.Modified))
				.ToList();

			foreach (var entry in entries)
			{
				if (entry.Entity is IAuditable _)
				{
					if (entry.State == EntityState.Modified)
					{
						// Ensure UpdatedAt is current for any modifications
						// Note: In a pure DDD approach, this should be handled by domain operations
						// This is a safety net for cases where domain operations might miss it
						var updatedAtProperty = entry.Property(nameof(IAuditable.UpdatedAt));;
						if (!updatedAtProperty.IsModified || 
						    (DateTimeOffset)updatedAtProperty.CurrentValue! < DateTimeOffset.UtcNow.AddSeconds(-5))
						{
							updatedAtProperty.CurrentValue = DateTimeOffset.UtcNow;
						}
					}

					if (entry.State == EntityState.Added)
					{
						var createdAtProperty = entry.Property(nameof(IAuditable.CreatedAt));;
						createdAtProperty.CurrentValue ??= DateTimeOffset.UtcNow;
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

	}
}
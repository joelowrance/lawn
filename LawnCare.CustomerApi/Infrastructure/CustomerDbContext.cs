using Microsoft.EntityFrameworkCore;

namespace LawnCare.CustomerApi.Infrastructure;
	
public class CustomerDbContext : DbContext
{
	readonly ILogger<CustomerDbContext> _logger;

	public CustomerDbContext(DbContextOptions options, ILogger<CustomerDbContext> logger) : base(options)
	{
		_logger = logger;
	}
	
	public DbSet<Customer> Customers { get; set; }
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
            
		modelBuilder.Entity<Customer>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
			entity.HasIndex(e => e.TenantId);
			entity.HasIndex(e => new { e.TenantId, e.Status });
                
			entity.Property(e => e.Id).ValueGeneratedOnAdd();
			entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
			entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
			// Configure enum conversions
			entity.Property(e => e.CustomerType)
				.HasConversion<int>();
                    
			entity.Property(e => e.Status)
				.HasConversion<int>();
		});
	}
        
	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		UpdateTimestamps();
		var result = await base.SaveChangesAsync(cancellationToken);
		_logger.LogDebug("Saved {ChangeCount} changes to database", result);
		return result;
	}
        
	private void UpdateTimestamps()
	{
		var entries = ChangeTracker.Entries<Customer>();
            
		foreach (var entry in entries)
		{
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Entity.CreatedAt = DateTime.UtcNow;
					entry.Entity.UpdatedAt = DateTime.UtcNow;
					break;
				case EntityState.Modified:
					entry.Entity.UpdatedAt = DateTime.UtcNow;
					break;
			}
		}
	}
}
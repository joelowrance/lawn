using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Domain.ValueObjects;

using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.CoreApi.Domain.Entities
{
	public class Job : AggregateRoot, IAuditable
	{
		private readonly List<JobLineItem> _services = new();
		private readonly List<JobNote> _notes = new();

		public JobId JobId { get; private init; }
		public JobStatus Status { get; private set; }
		public JobPriority Priority { get; private set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get;  set; }
		public DateTimeOffset? RequestedServiceDate { get; private set; }
		public Money JobCost { get; private set; }
		public LocationId LocationId { get; private set; }
		
		
		// Read-only collections exposed as IReadOnlyList
		public IReadOnlyList<JobLineItem> ServiceItems => _services.AsReadOnly();
		public IReadOnlyList<JobNote> Notes => _notes.AsReadOnly();

		
		// EF Core constructor
		private Job()
		{
			JobId = null!;
			RequestedServiceDate = null!;
			JobCost = Money.Zero();
			LocationId = null!;
		}
		
		public Job(DateTimeOffset? requestedServiceDate, JobPriority jobPriority, Money jobCost, LocationId locationId)
		{
			if (jobCost.Amount < 0)
			{
				throw new ArgumentException("Job cost cannot be negative", nameof(jobCost));
			}
			
			JobId = JobId.Create();
			CreatedAt = DateTimeOffset.UtcNow;
			Status = JobStatus.Pending;
			Priority = jobPriority;
			RequestedServiceDate = requestedServiceDate;
			JobCost = jobCost;
			LocationId = locationId;
			
			AddDomainEvent(new JobCreatedDomainEvent(JobId));
		}

		public void AddService(string serviceName, decimal quantity, string? comment, Money? price)
		{
			var service = new JobLineItem(JobId, serviceName, quantity, comment, price);
			_services.Add(service);
		}

			public void AddNote(string content)
	{
		var note = new JobNote(JobId, content.Trim());
		_notes.Add(note);
	}

	public void UpdateJobDetails(JobStatus? newStatus, JobPriority? newPriority, DateTimeOffset? newRequestedServiceDate, Money? newJobCost, List<JobLineItem>? newServiceItems, string reason)
	{
		if (string.IsNullOrWhiteSpace(reason))
		{
			throw new ArgumentException("Reason is required for job updates", nameof(reason));
		}

		bool hasChanges = false;

		if (newStatus.HasValue && newStatus.Value != Status)
		{
			Status = newStatus.Value;
			hasChanges = true;
		}

		if (newPriority.HasValue && newPriority.Value != Priority)
		{
			Priority = newPriority.Value;
			hasChanges = true;
		}

		if (newRequestedServiceDate != RequestedServiceDate)
		{
			RequestedServiceDate = newRequestedServiceDate;
			hasChanges = true;
		}

		if (newJobCost != null && newJobCost != JobCost)
		{
			JobCost = newJobCost;
			hasChanges = true;
		}

		if (newServiceItems != null)
		{
			_services.Clear();
			foreach (var serviceItem in newServiceItems)
			{
				_services.Add(serviceItem);
			}
			hasChanges = true;
		}

		if (hasChanges)
		{
			UpdatedAt = DateTimeOffset.UtcNow;
			AddNote($"Job updated: {reason.Trim()}");
		}
	}
	}
	
	public record JobCreatedDomainEvent(JobId JobId) : IDomainEvent;

	public class JobConfiguration : IEntityTypeConfiguration<Job>
	{
		
		public void Configure(EntityTypeBuilder<Job> builder)
		{
			builder.ToTable("Jobs", "public");
			
			builder.HasKey(x => x.JobId);
			
			builder.Property(x => x.JobId)
				.HasColumnName(nameof(JobId))
				.ValueGeneratedNever() // Always in code
				.HasConversion(x => x.Value, x => JobId.From(x));

			builder.Property(x => x.Status)
				.HasConversion<string>()
				.HasMaxLength(50)
				.IsRequired();
			
			builder.Property(x => x.Priority)
				.HasConversion<string>()
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(x => x.LocationId)
				.HasConversion(toDb => toDb.Value, fromDb => LocationId.From(fromDb))
				.IsRequired();
			

			builder.ComplexProperty(x => x.JobCost, cost =>
			{
				cost.Property(p => p.Amount)
					.HasPrecision(18, 2)
					.HasColumnName("JobCostAmount");

				cost.Property(p => p.Currency)
					.HasMaxLength(5)
					.HasDefaultValue("USD")
					.HasColumnName("JobCostCurrency");
			});
			
			// relationships 
			builder.HasMany(x => x.ServiceItems)
				.WithOne()
				.HasForeignKey(x => x.JobId)
				.HasPrincipalKey(x => x.JobId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("FK_JobLineItems_Job");
			
			builder.HasMany(x => x.Notes)
				.WithOne()
				.HasForeignKey(x => x.JobId)
				.HasPrincipalKey(x => x.JobId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("FK_JobNotes_Job");

			builder.HasOne<Location>()
				.WithMany()
				.HasForeignKey(x => x.LocationId)
				.OnDelete(DeleteBehavior.Cascade);
			
			// Backing fields for read only collections
			builder.Navigation(x => x.ServiceItems)
				.UsePropertyAccessMode(PropertyAccessMode.Field)
				.HasField("_services");
			
			builder.Navigation(x => x.Notes)
				.UsePropertyAccessMode(PropertyAccessMode.Field)
				.HasField("_notes");
			
			// Dont add to db
			builder.Ignore(x => x.DomainEvents);

			// indexes
			builder.HasIndex(j => j.Status)
				.HasDatabaseName("IX_Jobs_Status");

			builder.HasIndex(j => j.Priority)
				.HasDatabaseName("IX_Jobs_Priority");
			
			builder.HasIndex(j => j.LocationId)
				.HasDatabaseName("IX_Jobs_LocationId");
			
			
		}
	}
}
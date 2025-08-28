using LawnCare.CoreApi.Domain.Common;
using LawnCare.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OpenTelemetry.Instrumentation.AspNetCore;

namespace LawnCare.CoreApi.Domain.Entities
{
	//public enum JobType { LawnCare, LawnCareAndGarden, Garden, Other}

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
		}
	}

	public class Duration : ValueObject
	{
		public TimeSpan Value { get; }
		protected override IEnumerable<object> GetEqualityComponents()
		{
			throw new NotImplementedException();
		}
	}
	

	public class JobId : ValueObject
	{
		public Guid Value { get; }

		private JobId(Guid value)
		{
			Value = value;
		}

		public static JobId Create() => new(GuidHelper.NewId());
        
		public static JobId From(Guid value) => new(value);

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Value;
		}

		public override string ToString() => Value.ToString();
	}
}
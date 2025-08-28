using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawnCare.CoreApi.Domain.Entities
{

	
	
	public class JobLineItem : Entity, IAuditable
	{
		public string ServiceName { get; private set; } = null!;
		public decimal Quantity { get; private set; }
		public string? Comment { get; set; }
		public bool IsComplete { get; private set; }
		public Money Price { get; private set; }
		public JobId JobId { get; internal set; } = null!;

		//EF Core
		private JobLineItem()
		{
			Price = Money.Zero();
		}

		public JobLineItem(JobId jobId, string serviceName, decimal quantity, string? comment, Money? price)
		{
			ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
			Quantity = Math.Max(0, quantity);
			Comment = comment;
			JobId = jobId ?? throw new ArgumentNullException(nameof(jobId));
			Price = price ?? Money.Zero();
		}
		
		
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
	}
	
	public class JobLineItemConfiguration : IEntityTypeConfiguration<JobLineItem>
	{
		public void Configure(EntityTypeBuilder<JobLineItem> builder)
		{
			builder.ToTable("JobLineItems", "public");
			builder.HasKey(x => x.Id);
			builder.Property(x => x.ServiceName)
				.IsRequired()
				.HasMaxLength(255)
				.HasComment("The name of the service");
			
			builder.Property(x => x.Quantity)
				.IsRequired()
				.HasDefaultValue(1)
				.HasComment("The quantity of the service");
			
			builder.Property(x => x.Comment)
				.HasMaxLength(255)
				.HasComment("Any additional comments");

			builder.ComplexProperty(x => x.Price, price =>
			{
				price.Property(p => p.Amount)
					.HasPrecision(18, 2)
					.HasColumnName("Amount")
					.IsRequired()
					.HasComment("The price of the service");

				price.Property(p => p.Currency)
					.HasMaxLength(5)
					.IsRequired()
					.HasDefaultValue("USD")
					.HasComment("The currency of the price");
			});
			
			// Indexes
			builder.HasIndex(js => js.JobId)
				.HasDatabaseName("IX_JobServiceItems_JobId");

		}
	}
}
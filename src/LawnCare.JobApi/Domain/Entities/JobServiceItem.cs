using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.ValueObjects;

namespace LawnCare.JobApi.Domain.Entities;

public class JobServiceItem : Entity
{
	public string ServiceName { get; set; } = null!;
	public int Quantity { get; set; } = 1;
	public string? Comment { get; set; }
	public decimal Price { get; set; }
	public bool IsFulfilled { get; private set; }
	public JobId? JobId { get; internal set; }

	// For EF Core
	private JobServiceItem() { }

	// Original constructor (for backward compatibility)
	public JobServiceItem(string serviceName, int quantity, string? comment, decimal price)
	{
		ServiceName = serviceName;
		Quantity = quantity;
		Comment = comment;
		Price = price;
	}

	// New constructor with JobId
	public JobServiceItem(JobId jobId, string serviceName, int quantity, string? comment, decimal price)
		: this(serviceName, quantity, comment, price)
	{
		JobId = jobId ?? throw new ArgumentNullException(nameof(jobId));
	}
		
	public void MarkAsFulfilled()
	{
		IsFulfilled = true;
	}
}
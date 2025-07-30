using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.Entities;

public class JobServiceItem : Entity
{
	public string ServiceName { get; set; } = null!;
	public int Quantity { get; set; } = 1;
	public string? Comment { get; set; }
	public decimal Price { get; set; }
	public bool IsFulfilled { get; private set; }

	public JobServiceItem(string serviceName, int quantity, string? comment, decimal price)
	{
		ServiceName = serviceName;
		Quantity = quantity;
		Comment = comment;
		Price = price;
	}
		
	public void MarkAsFulfilled()
	{
		IsFulfilled = true;
	}
}
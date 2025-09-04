namespace LawnCare.JobApi.Domain.Common
{
	public interface IUnitOfWork
	{
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
	
}
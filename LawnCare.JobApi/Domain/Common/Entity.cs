namespace LawnCare.JobApi.Domain.Common
{
	public abstract class Entity
	{
		public Guid Id { get; private set; }  // Raw Guid
		
		public override bool Equals(object? obj)
		{
			return obj is Entity other && GetType() == other.GetType() && GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}
	}
}
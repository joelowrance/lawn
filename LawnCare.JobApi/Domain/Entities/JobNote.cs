using LawnCare.JobApi.Domain.Common;

namespace LawnCare.JobApi.Domain.Entities
{
	public class JobNote : Entity
	{
		public string Author { get; private set; }
		public string Content { get; private set; }
		public DateTime CreatedAt { get; private set; }

		public JobNote(string author, string content)
		{
			Author = author ?? throw new ArgumentNullException(nameof(author));
			Content = content ?? throw new ArgumentNullException(nameof(content));
			CreatedAt = DateTime.UtcNow;
		}
	}
}
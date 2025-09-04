using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.ValueObjects;

namespace LawnCare.JobApi.Domain.Entities
{
	public class JobNote : Entity
	{
		public string Author { get; private set; } = null!;
		public string Content { get; private set; } = null!;
		public DateTimeOffset CreatedAt { get; private set; }
		public JobId? JobId { get; internal set; }

		// Constructor for EF Core
		private JobNote() { }

		// Legacy constructor (for backward compatibility)
		public JobNote(string author, string content)
		{
			Author = author ?? throw new ArgumentNullException(nameof(author));
			Content = content ?? throw new ArgumentNullException(nameof(content));
			CreatedAt = DateTimeOffset.UtcNow;
		}

		public JobNote(JobId jobId, string author, string content) : this(author, content)
		{
			JobId = jobId ?? throw new ArgumentNullException(nameof(jobId));
		}
	}
}
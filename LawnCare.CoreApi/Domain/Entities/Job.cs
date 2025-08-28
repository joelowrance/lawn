using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Domain.ValueObjects;

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
		
		// Read-only collections exposed as IReadOnlyList
		public IReadOnlyList<JobLineItem> ServiceItems => _services.AsReadOnly();
		public IReadOnlyList<JobNote> Notes => _notes.AsReadOnly();

		
		// EF Core constructor
		private Job()
		{
			JobId = null!;
			RequestedServiceDate = null!;
			JobCost = Money.Zero();
		}
		
		public Job(DateTimeOffset? requestedServiceDate, JobPriority jobPriority, Money jobCost)
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
	}
}
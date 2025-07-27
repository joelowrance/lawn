using LawnCare.JobApi.Domain.Enums;
using LawnCare.JobApi.Domain.ValueObjects;
using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.DomainEvents;

namespace LawnCare.JobApi.Domain.Entities;

public class Job : AggregateRoot
{
	// Backing fields for EF Core access (private fields with public properties)
	private readonly List<JobRequirement> _requirements = new();
	private readonly List<JobNote> _notes = new();
        
        // Properties with private setters for encapsulation
        // Use null-forgiving operator for EF constructor, but these are never null in practice
        public JobId JobId { get; private set; } = null!;
        public TenantId TenantId { get; private set; } = null!;
        public CustomerId CustomerId { get; private set; } = null!;
        public string CustomerName { get; private set; } = null!;
        public ServiceAddress ServiceAddress { get; private set; } = null!;
        public ServiceType ServiceType { get; private set; } = null!;
        public JobStatus Status { get; private set; }
        public JobPriority Priority { get; private set; }
        public string Description { get; private set; } = null!;
        public string? SpecialInstructions { get; private set; } // Genuinely nullable
        public EstimatedDuration EstimatedDuration { get; private set; } = null!;
        public Money EstimatedCost { get; private set; } = null!;
        public Money? ActualCost { get; private set; } // Genuinely nullable
        public DateTime RequestedDate { get; private set; }
        public DateTime? ScheduledDate { get; private set; } // Genuinely nullable
        public DateTime? CompletedDate { get; private set; } // Genuinely nullable
        public TechnicianId? AssignedTechnicianId { get; private set; } // Genuinely nullable
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Read-only collections exposed as IReadOnlyList
        public IReadOnlyList<JobRequirement> Requirements => _requirements.AsReadOnly();
        public IReadOnlyList<JobNote> Notes => _notes.AsReadOnly();

        // CRITICAL: Private parameterless constructor for EF Core with nullable support
        // EF Core needs this to create instances when loading from database
        private Job()
        {
            // Use null-forgiving operator for properties that EF will populate
            // These are required in the domain but EF will set them from the database
            CustomerName = null!;
            Description = null!;
            ServiceAddress = null!;
            ServiceType = null!;
            EstimatedDuration = null!;
            EstimatedCost = null!;
            
            // Initialize collections - these are never null
            // (backing fields are already initialized above)
            
            // IMPORTANT: Don't initialize domain events here - we don't want events
            // when loading from database, only when business operations occur
        }
	public Job(TenantId tenantId, CustomerId customerId, string customerName, 
		ServiceAddress serviceAddress, ServiceType serviceType, 
		string description, DateTime requestedDate, 
		EstimatedDuration estimatedDuration, Money estimatedCost)
		: this()
	{
		ValidateCreationParameters(tenantId, customerId, customerName, serviceAddress, 
			serviceType, description, estimatedDuration, estimatedCost);

		
		JobId = JobId.Create();
		TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
		CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
		CustomerName = customerName ?? throw new ArgumentNullException(nameof(customerName));
		ServiceAddress = serviceAddress ?? throw new ArgumentNullException(nameof(serviceAddress));
		ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
		Description = description ?? throw new ArgumentNullException(nameof(description));
		RequestedDate = requestedDate;
		EstimatedDuration = estimatedDuration ?? throw new ArgumentNullException(nameof(estimatedDuration));
		EstimatedCost = estimatedCost ?? throw new ArgumentNullException(nameof(estimatedCost));
        
		Status = JobStatus.Pending;
		Priority = JobPriority.Normal;
		CreatedAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;

		AddDomainEvent(new JobCreatedEvent(JobId, TenantId, CustomerId));
	}
	
	private void ValidateCreationParameters(TenantId tenantId, CustomerId customerId, string customerName,
		ServiceAddress serviceAddress, ServiceType serviceType,
		string description, EstimatedDuration estimatedDuration, Money estimatedCost)
	{
			ArgumentNullException.ThrowIfNull(tenantId);
			ArgumentNullException.ThrowIfNull(customerId);
			ArgumentNullException.ThrowIfNull(serviceAddress);
			ArgumentNullException.ThrowIfNull(serviceType);
			ArgumentNullException.ThrowIfNull(estimatedDuration);
			ArgumentNullException.ThrowIfNull(estimatedCost);
			if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required", nameof(description));
			if (string.IsNullOrWhiteSpace(customerName)) throw new ArgumentException("Customer name is required", nameof(customerName));
		}

	public void ScheduleJob(DateTime scheduledDate, TechnicianId technicianId)
	{
		ValidateScheduling(scheduledDate, technicianId);

		var previousStatus = Status;
            
		ScheduledDate = scheduledDate;
		AssignedTechnicianId = technicianId;
		Status = JobStatus.Scheduled;
		UpdatedAt = DateTime.UtcNow;

		// Raise events for business operations
		AddDomainEvent(new JobScheduledEvent(JobId, TenantId, scheduledDate, technicianId));
	}

	public void StartJob()
	{
		if (Status != JobStatus.Scheduled)
			throw new InvalidOperationException("Only scheduled jobs can be started");

		Status = JobStatus.InProgress;
		UpdatedAt = DateTime.UtcNow;

		AddDomainEvent(new JobStartedEvent(JobId, TenantId, AssignedTechnicianId!));
	}

	public void CompleteJob(Money actualCost)
	{
		if (Status != JobStatus.InProgress)
			throw new InvalidOperationException("Only in-progress jobs can be completed");

		var previousStatus = Status;
		var completedAt = DateTime.UtcNow;

		Status = JobStatus.Completed;
		ActualCost = actualCost;
		CompletedDate = completedAt;
		UpdatedAt = completedAt;

		var actualDuration = CalculateActualDuration();
		AddDomainEvent(new JobCompletedEvent(JobId, TenantId, actualCost));
	}

	public void CancelJob(string reason)
	{
		if (Status == JobStatus.Completed)
			throw new InvalidOperationException("Completed jobs cannot be cancelled");

		Status = JobStatus.Cancelled;
		UpdatedAt = DateTime.UtcNow;
		AddNote("System", $"Job cancelled: {reason}");

		AddDomainEvent(new JobCancelledEvent(JobId, TenantId, reason));
	}

	public void AddRequirement(JobRequirement requirement)
	{
		if (requirement == null)
			throw new ArgumentNullException(nameof(requirement));

		_requirements.Add(requirement);
		UpdatedAt = DateTime.UtcNow;
	}

	public void AddNote(string author, string content)
	{
		var note = new JobNote(author, content);
		_notes.Add(note);
		UpdatedAt = DateTime.UtcNow;
	}

	public void UpdatePriority(JobPriority priority)
	{
		Priority = priority;
		UpdatedAt = DateTime.UtcNow;
	}
	
	// Internal method to update status without events (for loading from DB scenarios)
	internal void SetStatusWithoutEvents(JobStatus status)
	{
		Status = status;
		UpdatedAt = DateTime.UtcNow;
	}
	
	private void ValidateScheduling(DateTime scheduledDate, TechnicianId technicianId)
	{
		if (Status != JobStatus.Pending)
			throw new InvalidOperationException("Only pending jobs can be scheduled");
		if (scheduledDate <= DateTime.UtcNow)
			throw new ArgumentException("Scheduled date must be in the future", nameof(scheduledDate));
		if (technicianId == null)
			throw new ArgumentNullException(nameof(technicianId));
	}

	private TimeSpan CalculateActualDuration()
	{
		if (ScheduledDate.HasValue && CompletedDate.HasValue)
			return CompletedDate.Value - ScheduledDate.Value;
            
		return EstimatedDuration.Duration;
	}
}
using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.ValueObjects;

namespace LawnCare.JobApi.Domain.DomainEvents;

public record JobCreatedEvent(JobId JobId, TenantId TenantId, CustomerId CustomerId)
	: IDomainEvent;

public record JobScheduledEvent(JobId JobId, TenantId TenantId, DateTime ScheduledDate, TechnicianId TechnicianId)
	: IDomainEvent;

public record JobStartedEvent(JobId JobId, TenantId TenantId, TechnicianId TechnicianId)
	: IDomainEvent;

public record JobCompletedEvent(JobId JobId, TenantId TenantId, Money ActualCost)
	: IDomainEvent;

public record JobCancelledEvent(JobId JobId, TenantId TenantId, string Reason)
	: IDomainEvent;
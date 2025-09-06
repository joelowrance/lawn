namespace LawnCare.CoreApi.Domain.Common;


/// <summary>
/// Represents the Unit of Work pattern in Domain-Driven Design (DDD).
/// A Unit of Work coordinates and commits changes across one or more repositories
/// as a single atomic operation, forming the transactional boundary for a use case.
/// 
/// Purpose:
/// - Track changes to aggregates during a business operation.
/// - Ensure atomicity and consistency by committing all changes together or rolling them back.
/// - Provide a single point to persist data and optionally dispatch/persist domain events
///   (e.g., via an outbox) after a successful commit.
/// 
/// Guidance:
/// - Treat a unit of work as short-lived and scoped to an application service, command,
///   or request; ensure all repositories involved share the same instance.
/// - Expect concurrency exceptions to surface on save; handle retries or conflict resolution
///   at the application layer.
/// - Avoid performing external side effects until after a successful commit.
/// 
/// SaveChangesAsync:
/// - Persists all pending changes within the current unit of work and returns the number
///   of affected state entries.
/// </summary>

public interface IUnitOfWork
{
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
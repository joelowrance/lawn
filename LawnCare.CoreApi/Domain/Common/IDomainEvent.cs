namespace LawnCare.CoreApi.Domain.Common;

/// <summary>
/// Marker interface for a Domain Event in Domain-Driven Design (DDD).
/// A domain event represents something meaningful that has happened in the domain,
/// described from the business perspective and typically named in the past tense
/// (e.g., OrderConfirmed, CustomerRegistered).
/// 
/// Purpose:
/// - Capture significant state changes or decisions made within the domain model.
/// - Decouple side effects from the core model by allowing event handlers to react
///   asynchronously or in separate application layers, enabling eventual consistency.
/// - Serve as a reliable record that can be persisted and published after a successful
///   transaction (commonly via an outbox or similar mechanism).
/// 
/// Guidance:
/// - Treat events as immutable and focused; include only data necessary for consumers.
/// - Avoid leaking internal structures of aggregates; prefer IDs and value data.
/// - Consider including metadata (occurred-on timestamp, correlation/causation IDs) in
///   concrete implementations to aid tracing and monitoring.
/// </summary>
public interface IDomainEvent
{
}
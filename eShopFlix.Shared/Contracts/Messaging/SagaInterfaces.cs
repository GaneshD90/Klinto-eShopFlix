namespace Contracts.Messaging;

/// <summary>
/// Base interface for all saga commands.
/// Saga commands are sent (not published) to specific service endpoints.
/// </summary>
public interface ISagaCommand
{
    /// <summary>
    /// Correlation ID that links this command to a saga instance.
    /// </summary>
    Guid CorrelationId { get; }
}

/// <summary>
/// Generic saga command with expected response type.
/// </summary>
/// <typeparam name="TResponse">Expected response type</typeparam>
public interface ISagaCommand<TResponse> : ISagaCommand
{
}

/// <summary>
/// Base interface for all saga events.
/// Saga events are published to topics and consumed by the saga orchestrator.
/// </summary>
public interface ISagaEvent
{
    /// <summary>
    /// Correlation ID that links this event to a saga instance.
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}

/// <summary>
/// Marker interface for integration events (non-saga).
/// These are domain events published for general consumption.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}

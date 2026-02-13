namespace PaymentService.Application.Messaging;

/// <summary>
/// Interface for publishing integration events from PaymentService.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes an integration event to the message bus.
    /// </summary>
    /// <typeparam name="T">Type of the event</typeparam>
    /// <param name="event">The event to publish</param>
    /// <param name="ct">Cancellation token</param>
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Enqueues an integration event for later publishing (outbox pattern).
    /// </summary>
    /// <param name="eventName">Name of the event</param>
    /// <param name="event">The event data</param>
    /// <param name="ct">Cancellation token</param>
    Task EnqueueAsync(string eventName, object @event, CancellationToken ct = default);
}

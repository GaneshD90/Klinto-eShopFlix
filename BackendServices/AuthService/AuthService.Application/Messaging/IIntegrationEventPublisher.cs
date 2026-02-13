namespace AuthService.Application.Messaging;

/// <summary>
/// Interface for publishing integration events from AuthService.
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
}

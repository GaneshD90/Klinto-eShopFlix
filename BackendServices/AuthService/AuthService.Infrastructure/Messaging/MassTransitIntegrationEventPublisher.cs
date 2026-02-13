using AuthService.Application.Messaging;
using MassTransit;

namespace AuthService.Infrastructure.Messaging;

/// <summary>
/// MassTransit implementation of IIntegrationEventPublisher for AuthService.
/// </summary>
public sealed class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        await _publishEndpoint.Publish(@event, ct);
    }
}

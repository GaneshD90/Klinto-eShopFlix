using AuthService.Application.Messaging;
using Contracts.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Messaging;

/// <summary>
/// MassTransit + Azure Service Bus registration for AuthService.
/// AuthService publishes user lifecycle events (UserRegisteredV1, UserUpdatedV1, UserDeletedV1).
/// </summary>
public static class MassTransitRegistration
{
    public static IServiceCollection AddAuthServiceMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEShopFlixMessaging(configuration, bus =>
        {
            // AuthService is primarily a producer service.
            // Publishes: UserRegisteredV1, UserUpdatedV1, UserDeletedV1, UserEmailVerifiedV1, UserLoggedInV1
        });

        // Register the integration event publisher
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

        return services;
    }
}

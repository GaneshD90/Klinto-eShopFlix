using Contracts.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CartService.Infrastructure.Messaging.Consumers;

namespace CartService.Infrastructure.Messaging;

/// <summary>
/// MassTransit + Azure Service Bus registration for CartService.
/// Saga participants: handles cart commands from Checkout Saga.
/// Event consumers: deactivates cart after order, handles product updates, inventory releases, and user events.
/// </summary>
public static class MassTransitRegistration
{
    public static IServiceCollection AddCartServiceMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEShopFlixMessaging(configuration, bus =>
        {
            // Saga command consumer (orchestration)
            bus.AddConsumer<DeactivateCartForCheckoutConsumer>();
            
            // Event consumers (choreography)
            bus.AddConsumer<OrderFromCartCreatedConsumer>();
            bus.AddConsumer<ProductUpdatedConsumer>();
            bus.AddConsumer<InventoryReleasedConsumer>();
            
            // User event consumers
            bus.AddConsumer<UserRegisteredConsumer>();
            bus.AddConsumer<UserUpdatedConsumer>();
        });

        return services;
    }
}


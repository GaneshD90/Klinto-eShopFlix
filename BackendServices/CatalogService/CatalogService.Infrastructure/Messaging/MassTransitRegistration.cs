using Contracts.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CatalogService.Infrastructure.Messaging.Consumers;

namespace CatalogService.Infrastructure.Messaging;

/// <summary>
/// MassTransit + Azure Service Bus registration for CatalogService.
/// Consumers: updates local inventory snapshots from StockService events.
/// </summary>
public static class MassTransitRegistration
{
    public static IServiceCollection AddCatalogServiceMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEShopFlixMessaging(configuration, bus =>
        {
            // Inventory sync consumer - handles Reserved, Released, Committed events
            bus.AddConsumer<InventoryChangedConsumer>();
        });

        return services;
    }
}


using Contracts.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockService.Infrastructure.Messaging.Consumers;

namespace StockService.Infrastructure.Messaging;

/// <summary>
/// MassTransit + Azure Service Bus registration for StockService.
/// Saga participants: handles inventory commands from Checkout, Cancellation, and Return sagas.
/// Event consumers: reserves inventory on order creation, releases on payment failure or cancellation.
/// Catalog sync: creates stock items when products are added.
/// </summary>
public static class MassTransitRegistration
{
    public static IServiceCollection AddStockServiceMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEShopFlixMessaging(configuration, bus =>
        {
            // Checkout Saga command consumers
            bus.AddConsumer<ReserveInventoryForCheckoutConsumer>();
            bus.AddConsumer<ReleaseInventoryForCheckoutConsumer>();
            
            // Cancellation Saga command consumers
            bus.AddConsumer<ReleaseInventoryForCancellationConsumer>();
            
            // Return/Refund Saga command consumers
            bus.AddConsumer<RestockReturnedItemsConsumer>();
            
            // Event consumers (choreography)
            bus.AddConsumer<OrderCreatedConsumer>();
            bus.AddConsumer<PaymentFailedConsumer>();
            bus.AddConsumer<OrderCancelledConsumer>();
            bus.AddConsumer<ReturnRequestedConsumer>();
            
            // Catalog sync consumer
            bus.AddConsumer<ProductCreatedConsumer>();
        });

        return services;
    }
}


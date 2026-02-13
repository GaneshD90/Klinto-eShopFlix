using Contracts.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Infrastructure.Messaging.Consumers;

namespace PaymentService.Infrastructure.Messaging;

/// <summary>
/// MassTransit + Azure Service Bus registration for PaymentService.
/// Saga participants: handles payment commands from Checkout, Cancellation, and Return sagas.
/// Event consumers: handles inventory reserved, order confirmed, and order cancelled events.
/// </summary>
public static class MassTransitRegistration
{
    public static IServiceCollection AddPaymentServiceMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEShopFlixMessaging(configuration, bus =>
        {
            // Checkout Saga command consumer
            bus.AddConsumer<AuthorizePaymentForCheckoutConsumer>();
            
            // Cancellation Saga command consumer
            bus.AddConsumer<ProcessRefundForCancellationConsumer>();
            
            // Return/Refund Saga command consumer
            bus.AddConsumer<ProcessReturnRefundConsumer>();
            
            // Event consumers (choreography)
            bus.AddConsumer<InventoryReservedConsumer>();
            bus.AddConsumer<OrderConfirmedConsumer>();
            bus.AddConsumer<OrderCancelledConsumer>();
        });

        return services;
    }
}


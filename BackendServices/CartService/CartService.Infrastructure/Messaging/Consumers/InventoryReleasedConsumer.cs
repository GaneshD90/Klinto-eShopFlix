using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using CartService.Application.Repositories;

namespace CartService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes InventoryReleasedV2 as saga compensation.
/// When inventory is released (e.g., payment failed), this could restore the cart
/// to active state if the order was cancelled before completion.
/// </summary>
public sealed class InventoryReleasedConsumer : IConsumer<InventoryReleasedV2>
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<InventoryReleasedConsumer> _logger;

    public InventoryReleasedConsumer(
        ICartRepository cartRepository,
        ILogger<InventoryReleasedConsumer> logger)
    {
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReleasedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "InventoryReleasedConsumer received InventoryReleasedV2 for OrderId={OrderId}, Reason={Reason}",
            msg.OrderId, msg.ReleaseReason);

        // Note: In a full implementation, you would:
        // 1. Lookup the cart associated with this order
        // 2. Determine if the cart should be reactivated based on business rules
        // 3. Optionally notify the customer that their cart is available again

        // For now, we log the event for audit purposes
        // Reactivating carts automatically could cause confusion if the customer
        // has already started a new cart

        _logger.LogInformation(
            "Inventory released for OrderId={OrderId} - cart restoration not implemented (business decision)",
            msg.OrderId);

        await Task.CompletedTask;
    }
}

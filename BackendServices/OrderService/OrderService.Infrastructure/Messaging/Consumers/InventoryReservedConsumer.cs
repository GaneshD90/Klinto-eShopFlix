using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes InventoryReservedV2 as part of the order saga.
/// Updates the order's fulfillment status when inventory is reserved.
/// </summary>
public sealed class InventoryReservedConsumer : IConsumer<InventoryReservedV2>
{
    private readonly OrderServiceDbContext _dbContext;
    private readonly ILogger<InventoryReservedConsumer> _logger;

    public InventoryReservedConsumer(
        OrderServiceDbContext dbContext,
        ILogger<InventoryReservedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReservedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "InventoryReservedConsumer received InventoryReservedV2 for OrderId={OrderId}, ReservationId={ReservationId}, Quantity={Quantity}",
            msg.OrderId, msg.ReservationId, msg.ReservedQuantity);

        try
        {
            // Find the order and update fulfillment status
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.OrderId == msg.OrderId, context.CancellationToken);

            if (order == null)
            {
                _logger.LogWarning(
                    "Order {OrderId} not found when processing InventoryReservedV2",
                    msg.OrderId);
                return;
            }

            // Update fulfillment status to indicate stock is reserved
            if (order.FulfillmentStatus == "Unfulfilled" || order.FulfillmentStatus == "Pending")
            {
                order.FulfillmentStatus = "StockReserved";
                order.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation(
                    "Order {OrderId} fulfillment status updated to StockReserved",
                    msg.OrderId);
            }

            // Optionally update order items with reservation IDs
            if (msg.ReservationId != Guid.Empty)
            {
                var orderItems = await _dbContext.OrderItems
                    .Where(i => i.OrderId == msg.OrderId && i.StockReservationId == null)
                    .ToListAsync(context.CancellationToken);

                foreach (var item in orderItems)
                {
                    item.StockReservationId = msg.ReservationId;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                if (orderItems.Any())
                {
                    await _dbContext.SaveChangesAsync(context.CancellationToken);
                    _logger.LogInformation(
                        "Updated {Count} order items with ReservationId={ReservationId}",
                        orderItems.Count, msg.ReservationId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing InventoryReservedV2 for OrderId={OrderId}", msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}

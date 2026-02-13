using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Commands;
using StockService.Application.CQRS;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes OrderCreatedV2 as part of the order saga.
/// Attempts to reserve inventory for each order line.
/// Publishes InventoryReservedV2 on success, or InventoryReservationFailedV2 on failure.
/// </summary>
public sealed class OrderCreatedConsumer : IConsumer<OrderCreatedV2>
{
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IDispatcher dispatcher, ILogger<OrderCreatedConsumer> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "OrderCreatedConsumer received OrderCreatedV2 for OrderId={OrderId}, OrderNumber={OrderNumber}, Lines={LineCount}",
            msg.OrderId, msg.OrderNumber, msg.Lines?.Count() ?? 0);

        // Validate that we have order lines to process
        var lines = msg.Lines?.ToList() ?? new List<Contracts.DTOs.OrderLineV2Dto>();
        if (!lines.Any())
        {
            _logger.LogWarning(
                "OrderCreatedV2 for OrderId={OrderId} has no lines - cannot reserve inventory",
                msg.OrderId);

            await context.Publish(new InventoryReservationFailedV2(
                OrderId: msg.OrderId,
                OrderNumber: msg.OrderNumber,
                CustomerId: msg.CustomerId,
                Reason: "Order has no line items to reserve",
                OccurredAt: DateTime.UtcNow));

            return;
        }

        try
        {
            var reservationIds = new List<Guid>();

            // Reserve inventory for each line in the order
            foreach (var line in lines)
            {
                var command = new ReserveStockCommand(
                    ProductId: line.ProductId,
                    VariationId: line.VariationId,
                    WarehouseId: null, // auto-allocate
                    CartId: msg.CartId,
                    OrderId: msg.OrderId,
                    CustomerId: msg.CustomerId,
                    Quantity: line.Quantity,
                    ReservationType: "Order",
                    IdempotencyKey: $"saga-reserve-{msg.OrderId}-{line.ProductId}");

                var result = await _dispatcher.SendAsync(command, context.CancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Stock reservation failed for OrderId={OrderId}, ProductId={ProductId}: {Message}",
                        msg.OrderId, line.ProductId, result.Message);

                    // Compensation: Release any reservations we already made
                    foreach (var reservationId in reservationIds)
                    {
                        try
                        {
                            await _dispatcher.SendAsync(
                                new ReleaseReservationCommand(reservationId, "Partial reservation rollback"),
                                context.CancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to release reservation {ReservationId} during rollback", reservationId);
                        }
                    }

                    await context.Publish(new InventoryReservationFailedV2(
                        OrderId: msg.OrderId,
                        OrderNumber: msg.OrderNumber,
                        CustomerId: msg.CustomerId,
                        Reason: result.Message ?? $"Insufficient stock for product {line.ProductId}",
                        OccurredAt: DateTime.UtcNow));

                    return;
                }

                reservationIds.Add(result.ReservationId);
            }

            // All lines reserved successfully - publish aggregate event
            var totalReserved = lines.Sum(l => l.Quantity);

            await context.Publish(new InventoryReservedV2(
                ReservationId: reservationIds.First(), // Primary reservation ID
                OrderId: msg.OrderId,
                ProductId: Guid.Empty, // Aggregate - multiple products
                VariationId: null,
                ReservedQuantity: totalReserved,
                WarehouseId: null,
                Sku: null,
                ExpiresAt: DateTime.UtcNow.AddMinutes(30),
                OccurredAt: DateTime.UtcNow));

            _logger.LogInformation(
                "Inventory reserved successfully for OrderId={OrderId}, OrderNumber={OrderNumber}, TotalQuantity={TotalQuantity}",
                msg.OrderId, msg.OrderNumber, totalReserved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error reserving inventory for OrderId={OrderId}", msg.OrderId);

            await context.Publish(new InventoryReservationFailedV2(
                OrderId: msg.OrderId,
                OrderNumber: msg.OrderNumber,
                CustomerId: msg.CustomerId,
                Reason: $"Internal error: {ex.Message}",
                OccurredAt: DateTime.UtcNow));
        }
    }
}

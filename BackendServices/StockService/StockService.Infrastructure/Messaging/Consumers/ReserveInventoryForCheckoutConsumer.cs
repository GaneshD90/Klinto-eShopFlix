using Contracts.Sagas.Checkout;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Repositories;
using StockService.Domain.Entities;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles ReserveInventoryForCheckout command.
/// Called by the Checkout Saga orchestrator to reserve inventory for an order.
/// </summary>
public sealed class ReserveInventoryForCheckoutConsumer : IConsumer<ReserveInventoryForCheckout>
{
    private readonly IStockRepository _stockRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<ReserveInventoryForCheckoutConsumer> _logger;

    public ReserveInventoryForCheckoutConsumer(
        IStockRepository stockRepository,
        IReservationRepository reservationRepository,
        IWarehouseRepository warehouseRepository,
        ILogger<ReserveInventoryForCheckoutConsumer> logger)
    {
        _stockRepository = stockRepository;
        _reservationRepository = reservationRepository;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReserveInventoryForCheckout> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "ReserveInventoryForCheckoutConsumer processing command for OrderId={OrderId}, CorrelationId={CorrelationId}, LineCount={LineCount}",
            cmd.OrderId, cmd.CorrelationId, cmd.Lines?.Count() ?? 0);

        var reservationId = Guid.NewGuid();
        var totalQuantityReserved = 0;
        var failureReason = string.Empty;
        var success = true;

        try
        {
            // Get default warehouse
            var warehouses = await _warehouseRepository.GetAllActiveAsync(context.CancellationToken);
            var defaultWarehouse = warehouses.OrderBy(w => w.Priority).FirstOrDefault();

            if (defaultWarehouse == null)
            {
                failureReason = "No active warehouse available";
                success = false;
            }
            else if (cmd.Lines == null || !cmd.Lines.Any())
            {
                failureReason = "No items to reserve";
                success = false;
            }
            else
            {
                // Try to reserve each line item
                foreach (var line in cmd.Lines)
                {
                    var stockItem = await _stockRepository.GetByProductAndWarehouseAsync(
                        line.ProductId, line.VariationId, defaultWarehouse.WarehouseId, context.CancellationToken);

                    if (stockItem == null)
                    {
                        _logger.LogWarning(
                            "No stock item found for ProductId={ProductId} in Warehouse={WarehouseId}",
                            line.ProductId, defaultWarehouse.WarehouseId);
                        failureReason = $"Product {line.ProductName} not found in inventory";
                        success = false;
                        break;
                    }

                    if (stockItem.AvailableQuantity < line.Quantity)
                    {
                        _logger.LogWarning(
                            "Insufficient stock for ProductId={ProductId}. Available={Available}, Requested={Requested}",
                            line.ProductId, stockItem.AvailableQuantity, line.Quantity);
                        failureReason = $"Insufficient stock for {line.ProductName}. Available: {stockItem.AvailableQuantity}, Requested: {line.Quantity}";
                        success = false;
                        break;
                    }

                    // Create reservation
                    var reservation = new StockReservation
                    {
                        ReservationId = Guid.NewGuid(),
                        StockItemId = stockItem.StockItemId,
                        OrderId = cmd.OrderId,
                        CartId = cmd.CartId,
                        CustomerId = cmd.CustomerId,
                        ReservedQuantity = line.Quantity,
                        ReservationStatus = "Reserved",
                        ReservationType = "Order",
                        ReservedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30), // 30 min reservation window
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _reservationRepository.CreateAsync(reservation, context.CancellationToken);

                    // Update stock item
                    stockItem.AvailableQuantity -= line.Quantity;
                    stockItem.ReservedQuantity += line.Quantity;
                    stockItem.UpdatedAt = DateTime.UtcNow;
                    await _stockRepository.UpdateAsync(stockItem, context.CancellationToken);

                    totalQuantityReserved += line.Quantity;

                    _logger.LogInformation(
                        "Reserved {Quantity} of ProductId={ProductId} for OrderId={OrderId}",
                        line.Quantity, line.ProductId, cmd.OrderId);
                }
            }

            if (success)
            {
                // Publish success event
                await context.Publish(new InventoryReservedForCheckout(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    ReservationId: reservationId,
                    TotalQuantityReserved: totalQuantityReserved,
                    ExpiresAt: DateTime.UtcNow.AddMinutes(30),
                    OccurredAt: DateTime.UtcNow));

                _logger.LogInformation(
                    "Inventory reserved successfully for OrderId={OrderId}, TotalQuantity={TotalQuantity}",
                    cmd.OrderId, totalQuantityReserved);
            }
            else
            {
                // Publish failure event
                await context.Publish(new InventoryReservationFailedForCheckout(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    Reason: failureReason,
                    OccurredAt: DateTime.UtcNow));

                _logger.LogWarning(
                    "Inventory reservation failed for OrderId={OrderId}: {Reason}",
                    cmd.OrderId, failureReason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error reserving inventory for OrderId={OrderId}", cmd.OrderId);

            // Publish failure event
            await context.Publish(new InventoryReservationFailedForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                Reason: $"System error: {ex.Message}",
                OccurredAt: DateTime.UtcNow));
        }
    }
}

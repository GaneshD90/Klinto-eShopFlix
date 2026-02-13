using Contracts.Sagas.ReturnRefund;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Repositories;
using StockService.Domain.Entities;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles RestockReturnedItems command.
/// Adds returned items back into inventory.
/// </summary>
public sealed class RestockReturnedItemsConsumer : IConsumer<RestockReturnedItems>
{
    private readonly IStockRepository _stockRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<RestockReturnedItemsConsumer> _logger;

    public RestockReturnedItemsConsumer(
        IStockRepository stockRepository,
        IWarehouseRepository warehouseRepository,
        ILogger<RestockReturnedItemsConsumer> logger)
    {
        _stockRepository = stockRepository;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RestockReturnedItems> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "RestockReturnedItemsConsumer processing for OrderId={OrderId}, ReturnId={ReturnId}",
            cmd.OrderId, cmd.ReturnId);

        var itemsRestocked = 0;
        Guid? warehouseId = cmd.PreferredWarehouseId;

        try
        {
            // Get default warehouse if not specified
            if (!warehouseId.HasValue)
            {
                var warehouses = await _warehouseRepository.GetAllActiveAsync(context.CancellationToken);
                var defaultWarehouse = warehouses.OrderBy(w => w.Priority).FirstOrDefault();
                warehouseId = defaultWarehouse?.WarehouseId;
            }

            if (!warehouseId.HasValue)
            {
                await context.Publish(new RestockingFailed(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    ReturnId: cmd.ReturnId,
                    Reason: "No active warehouse available for restocking",
                    OccurredAt: DateTime.UtcNow));
                return;
            }

            foreach (var item in cmd.Items ?? Enumerable.Empty<ReturnLineItem>())
            {
                // Only restock items in good condition
                if (item.ReturnCondition != "Good")
                {
                    _logger.LogInformation(
                        "Skipping restock for ProductId={ProductId}, Condition={Condition}",
                        item.ProductId, item.ReturnCondition);
                    continue;
                }

                var stockItem = await _stockRepository.GetByProductAndWarehouseAsync(
                    item.ProductId, item.VariationId, warehouseId.Value, context.CancellationToken);

                if (stockItem != null)
                {
                    stockItem.AvailableQuantity += item.Quantity;
                    stockItem.UpdatedAt = DateTime.UtcNow;
                    stockItem.LastRestockedAt = DateTime.UtcNow;
                    await _stockRepository.UpdateAsync(stockItem, context.CancellationToken);
                    itemsRestocked += item.Quantity;

                    _logger.LogInformation(
                        "Restocked {Quantity} of ProductId={ProductId} to Warehouse={WarehouseId}",
                        item.Quantity, item.ProductId, warehouseId);
                }
                else
                {
                    _logger.LogWarning(
                        "No stock item found for ProductId={ProductId} in Warehouse={WarehouseId}",
                        item.ProductId, warehouseId);
                }
            }

            await context.Publish(new ItemsRestockedEvent(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ReturnId: cmd.ReturnId,
                RestockedCount: itemsRestocked,
                WarehouseId: warehouseId,
                OccurredAt: DateTime.UtcNow));

            _logger.LogInformation(
                "Restocked {Count} items for OrderId={OrderId}",
                itemsRestocked, cmd.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error restocking items for OrderId={OrderId}", cmd.OrderId);

            await context.Publish(new RestockingFailed(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ReturnId: cmd.ReturnId,
                Reason: $"Restocking error: {ex.Message}",
                OccurredAt: DateTime.UtcNow));
        }
    }
}

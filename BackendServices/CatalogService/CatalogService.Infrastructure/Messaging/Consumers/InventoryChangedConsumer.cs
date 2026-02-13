using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes InventoryReservedV2 and InventoryReleasedV2 to update local inventory snapshots.
/// CatalogService maintains denormalized inventory data for fast product availability queries.
/// </summary>
public sealed class InventoryChangedConsumer : 
    IConsumer<InventoryReservedV2>,
    IConsumer<InventoryReleasedV2>,
    IConsumer<InventoryCommittedV2>
{
    private readonly ILogger<InventoryChangedConsumer> _logger;

    public InventoryChangedConsumer(ILogger<InventoryChangedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReservedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "InventoryChangedConsumer received InventoryReservedV2: ProductId={ProductId}, Quantity={Quantity}, OrderId={OrderId}",
            msg.ProductId, msg.ReservedQuantity, msg.OrderId);

        // In a full implementation:
        // 1. Update InventorySnapshot table with new reserved quantity
        // 2. Recalculate available quantity for display
        // await _catalogDbContext.Procedures.SP_InventorySnapshot_UpsertAsync(...)

        // For now, log for audit - actual inventory data is fetched from StockService
        await Task.CompletedTask;
    }

    public async Task Consume(ConsumeContext<InventoryReleasedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "InventoryChangedConsumer received InventoryReleasedV2: ProductId={ProductId}, Quantity={Quantity}, OrderId={OrderId}, Reason={Reason}",
            msg.ProductId, msg.Quantity, msg.OrderId, msg.ReleaseReason);

        // In a full implementation:
        // 1. Update InventorySnapshot table - reduce reserved, increase available
        // 2. This restores availability for other customers
        
        await Task.CompletedTask;
    }

    public async Task Consume(ConsumeContext<InventoryCommittedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "InventoryChangedConsumer received InventoryCommittedV2: ProductId={ProductId}, Quantity={Quantity}, OrderId={OrderId}",
            msg.ProductId, msg.Quantity, msg.OrderId);

        // In a full implementation:
        // 1. Update InventorySnapshot - reduce both reserved and total
        // 2. This reflects actual inventory decrease after order fulfillment
        
        await Task.CompletedTask;
    }
}

using Contracts.Events.V1;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Commands;
using StockService.Application.CQRS;
using StockService.Application.Repositories;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes ProductCreatedIntegrationEvent (mapped to ProductUpdatedV1 with IsActive=true).
/// Creates placeholder stock items when new products are added to the catalog.
/// This ensures inventory can be managed for new products immediately.
/// </summary>
public sealed class ProductCreatedConsumer : IConsumer<ProductUpdatedV1>
{
    private readonly IStockRepository _stockRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(
        IStockRepository stockRepository,
        IWarehouseRepository warehouseRepository,
        ILogger<ProductCreatedConsumer> logger)
    {
        _stockRepository = stockRepository;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductUpdatedV1> context)
    {
        var msg = context.Message;
        
        // Only process active products
        if (!msg.IsActive)
        {
            _logger.LogDebug(
                "ProductUpdatedV1 for ProductId={ProductId} is inactive - skipping stock item creation",
                msg.ProductId);
            return;
        }

        _logger.LogInformation(
            "ProductCreatedConsumer received ProductUpdatedV1 for ProductId={ProductId}, SKU={SKU}, Name={Name}",
            msg.ProductId, msg.SKU, msg.Name);

        try
        {
            // Get default warehouse (highest priority active warehouse)
            var warehouses = await _warehouseRepository.GetAllActiveAsync(context.CancellationToken);
            var defaultWarehouse = warehouses.OrderBy(w => w.Priority).FirstOrDefault();

            if (defaultWarehouse == null)
            {
                _logger.LogWarning(
                    "No active warehouse found - cannot create stock item for ProductId={ProductId}",
                    msg.ProductId);
                return;
            }

            // Convert int ProductId to Guid for StockService
            var productGuid = ConvertIntToGuid(msg.ProductId);

            // Check if stock item already exists
            var existing = await _stockRepository.GetByProductAndWarehouseAsync(
                productGuid, null, defaultWarehouse.WarehouseId, context.CancellationToken);

            if (existing != null)
            {
                _logger.LogDebug(
                    "Stock item already exists for ProductId={ProductId} in Warehouse={WarehouseId}",
                    msg.ProductId, defaultWarehouse.WarehouseId);
                return;
            }

            // Create placeholder stock item with zero quantity
            var stockItem = new StockService.Domain.Entities.StockItem
            {
                StockItemId = Guid.NewGuid(),
                ProductId = productGuid,
                VariationId = null,
                WarehouseId = defaultWarehouse.WarehouseId,
                Sku = msg.SKU,
                AvailableQuantity = 0, // Placeholder - actual stock added via PO or manual
                ReservedQuantity = 0,
                InTransitQuantity = 0,
                DamagedQuantity = 0,
                MinimumStockLevel = 10, // Default minimum
                MaximumStockLevel = 1000, // Default maximum
                ReorderQuantity = 50, // Default reorder quantity
                UnitCost = msg.Price * 0.6m, // Estimate cost at 60% of price
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _stockRepository.CreateAsync(stockItem, context.CancellationToken);

            _logger.LogInformation(
                "Created placeholder stock item {StockItemId} for ProductId={ProductId} in Warehouse={WarehouseName}",
                stockItem.StockItemId, msg.ProductId, defaultWarehouse.WarehouseName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating stock item for ProductId={ProductId}", msg.ProductId);
            // Don't throw - stock creation failure shouldn't block catalog updates
        }
    }

    /// <summary>
    /// Converts an integer product ID to a deterministic GUID.
    /// This matches the conversion used elsewhere in the system.
    /// </summary>
    private static Guid ConvertIntToGuid(int id)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(id).CopyTo(bytes, 0);
        bytes[4] = 0xE5; // eShop marker
        bytes[5] = 0x0F; // Flix marker
        return new Guid(bytes);
    }
}

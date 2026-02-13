using Contracts.Events.V1;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CartService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes ProductUpdatedV1 when a product is updated in CatalogService.
/// Updates any cached product data in active carts (e.g., name, price changes).
/// 
/// Note: In a full implementation, this would:
/// 1. Find all active carts containing this product
/// 2. Update the cached ProductName, UnitPrice if configured to track price changes
/// 3. Optionally notify customers if prices changed significantly
/// </summary>
public sealed class ProductUpdatedConsumer : IConsumer<ProductUpdatedV1>
{
    private readonly ILogger<ProductUpdatedConsumer> _logger;

    public ProductUpdatedConsumer(ILogger<ProductUpdatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductUpdatedV1> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "ProductUpdatedConsumer received ProductUpdatedV1 for ProductId={ProductId}, Name={Name}, Price={Price}, IsActive={IsActive}",
            msg.ProductId, msg.Name, msg.Price, msg.IsActive);

        // If product is deactivated, we might want to:
        // 1. Mark cart items as unavailable
        // 2. Notify customers with this item in their cart
        if (!msg.IsActive)
        {
            _logger.LogWarning(
                "Product {ProductId} ({Name}) has been deactivated - carts with this item should be notified",
                msg.ProductId, msg.Name);
        }

        // Price change handling:
        // In a real implementation, you might update cart item prices
        // or flag items where price has changed since they were added
        
        // For now, we log for audit purposes
        // The cart will fetch fresh product data on next access via CatalogServiceClient

        await Task.CompletedTask;
    }
}

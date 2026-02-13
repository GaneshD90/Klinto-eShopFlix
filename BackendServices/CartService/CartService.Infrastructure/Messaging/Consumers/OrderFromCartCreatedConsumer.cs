using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using CartService.Application.CQRS;
using CartService.Application.Carts.Commands;

namespace CartService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes OrderCreatedV2 when an order is created from a cart.
/// Deactivates the cart so it cannot be modified or reused.
/// </summary>
public sealed class OrderFromCartCreatedConsumer : IConsumer<OrderCreatedV2>
{
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<OrderFromCartCreatedConsumer> _logger;

    public OrderFromCartCreatedConsumer(
        IDispatcher dispatcher,
        ILogger<OrderFromCartCreatedConsumer> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedV2> context)
    {
        var msg = context.Message;

        // Only process if this order was created from a cart
        if (!msg.CartId.HasValue)
        {
            _logger.LogDebug(
                "OrderCreatedV2 for OrderId={OrderId} has no CartId - skipping cart deactivation",
                msg.OrderId);
            return;
        }

        _logger.LogInformation(
            "OrderFromCartCreatedConsumer received OrderCreatedV2 for OrderId={OrderId}, CartId={CartId}",
            msg.OrderId, msg.CartId);

        try
        {
            // CartService uses long for CartId, we need to convert from Guid
            // The CartId in OrderCreatedV2 is a Guid, but CartService internally uses long
            // We'll need to handle this mapping - for now, log and attempt deactivation
            
            // Note: This requires the CartService to have a way to lookup cart by Guid
            // or the OrderService to store the original long CartId
            // For demonstration, we'll use a deterministic conversion
            var cartIdLong = ConvertGuidToLongId(msg.CartId.Value);

            var command = new MakeInActiveCommand((int)cartIdLong);
            var result = await _dispatcher.Send(command, context.CancellationToken);

            if (result)
            {
                _logger.LogInformation(
                    "Cart {CartId} deactivated after order {OrderId} was created",
                    msg.CartId, msg.OrderId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to deactivate cart {CartId} for order {OrderId}",
                    msg.CartId, msg.OrderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deactivating cart {CartId} for order {OrderId}",
                msg.CartId, msg.OrderId);
            // Don't throw - cart deactivation failure shouldn't block order flow
        }
    }

    /// <summary>
    /// Converts a deterministic Guid back to the original long ID.
    /// This matches the conversion used in StockServiceClient.CreateDeterministicGuid.
    /// </summary>
    private static long ConvertGuidToLongId(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }
}

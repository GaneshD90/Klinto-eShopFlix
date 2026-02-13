using Contracts.Sagas.Checkout;
using MassTransit;
using Microsoft.Extensions.Logging;
using CartService.Application.Repositories;

namespace CartService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles DeactivateCartForCheckout command.
/// Called by the Checkout Saga orchestrator after order is confirmed.
/// </summary>
public sealed class DeactivateCartForCheckoutConsumer : IConsumer<DeactivateCartForCheckout>
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<DeactivateCartForCheckoutConsumer> _logger;

    public DeactivateCartForCheckoutConsumer(
        ICartRepository cartRepository,
        ILogger<DeactivateCartForCheckoutConsumer> logger)
    {
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeactivateCartForCheckout> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "DeactivateCartForCheckoutConsumer processing command for CartId={CartId}, OrderId={OrderId}, CorrelationId={CorrelationId}",
            cmd.CartId, cmd.OrderId, cmd.CorrelationId);

        try
        {
            // Convert Guid to long for CartService (uses long internally)
            var cartIdLong = ConvertGuidToLongId(cmd.CartId);

            // Deactivate the cart
            var success = await _cartRepository.MakeInActive((int)cartIdLong);

            if (success)
            {
                _logger.LogInformation(
                    "Cart {CartId} deactivated for OrderId={OrderId}",
                    cmd.CartId, cmd.OrderId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to deactivate cart {CartId} for OrderId={OrderId} - cart may already be inactive",
                    cmd.CartId, cmd.OrderId);
            }

            // Always publish success - cart deactivation failure shouldn't fail the saga
            await context.Publish(new CartDeactivatedForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                CartId: cmd.CartId,
                OccurredAt: DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deactivating cart {CartId} for OrderId={OrderId}",
                cmd.CartId, cmd.OrderId);

            // Still publish success - cart deactivation is not critical to order flow
            await context.Publish(new CartDeactivatedForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                CartId: cmd.CartId,
                OccurredAt: DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Converts a deterministic Guid back to the original long ID.
    /// </summary>
    private static long ConvertGuidToLongId(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }
}

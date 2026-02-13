using Contracts.Sagas.Checkout;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles ConfirmOrderForCheckout command.
/// Called by the Checkout Saga orchestrator to confirm the order after payment is authorized.
/// </summary>
public sealed class ConfirmOrderForCheckoutConsumer : IConsumer<ConfirmOrderForCheckout>
{
    private readonly OrderServiceDbContext _dbContext;
    private readonly ILogger<ConfirmOrderForCheckoutConsumer> _logger;

    public ConfirmOrderForCheckoutConsumer(
        OrderServiceDbContext dbContext,
        ILogger<ConfirmOrderForCheckoutConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ConfirmOrderForCheckout> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "ConfirmOrderForCheckoutConsumer processing command for OrderId={OrderId}, CorrelationId={CorrelationId}",
            cmd.OrderId, cmd.CorrelationId);

        try
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.OrderId == cmd.OrderId, context.CancellationToken);

            if (order == null)
            {
                _logger.LogError("Order {OrderId} not found", cmd.OrderId);
                
                await context.Publish(new OrderConfirmationFailedForCheckout(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    Reason: "Order not found",
                    OccurredAt: DateTime.UtcNow));
                return;
            }

            // Update order status
            order.OrderStatus = "Confirmed";
            order.PaymentStatus = "Authorized";
            order.ConfirmedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(context.CancellationToken);

            // Publish success event
            await context.Publish(new OrderConfirmedForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ConfirmedAt: order.ConfirmedAt.Value,
                OccurredAt: DateTime.UtcNow));

            _logger.LogInformation(
                "Order {OrderId} confirmed successfully",
                cmd.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error confirming order {OrderId}", cmd.OrderId);

            await context.Publish(new OrderConfirmationFailedForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                Reason: $"System error: {ex.Message}",
                OccurredAt: DateTime.UtcNow));
        }
    }
}

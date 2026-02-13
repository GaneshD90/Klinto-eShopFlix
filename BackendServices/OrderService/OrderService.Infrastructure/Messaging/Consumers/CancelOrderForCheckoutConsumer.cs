using Contracts.Sagas.Checkout;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles CancelOrderForCheckout command.
/// Called by the Checkout Saga orchestrator as compensation when inventory or payment fails.
/// </summary>
public sealed class CancelOrderForCheckoutConsumer : IConsumer<CancelOrderForCheckout>
{
    private readonly OrderServiceDbContext _dbContext;
    private readonly ILogger<CancelOrderForCheckoutConsumer> _logger;

    public CancelOrderForCheckoutConsumer(
        OrderServiceDbContext dbContext,
        ILogger<CancelOrderForCheckoutConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CancelOrderForCheckout> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "CancelOrderForCheckoutConsumer processing command for OrderId={OrderId}, CorrelationId={CorrelationId}, Reason={Reason}",
            cmd.OrderId, cmd.CorrelationId, cmd.CancellationReason);

        try
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.OrderId == cmd.OrderId, context.CancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for cancellation", cmd.OrderId);
            }
            else
            {
                // Update order status
                order.OrderStatus = "Cancelled";
                order.CancelledAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                // Store cancellation reason in CustomerNotes or a dedicated field
                order.CustomerNotes = string.IsNullOrEmpty(order.CustomerNotes)
                    ? $"Saga cancellation: {cmd.CancellationReason}"
                    : $"{order.CustomerNotes}\nSaga cancellation: {cmd.CancellationReason}";

                await _dbContext.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation(
                    "Order {OrderId} cancelled: {Reason}",
                    cmd.OrderId, cmd.CancellationReason);
            }

            // Always publish completion - compensation should not fail
            await context.Publish(new OrderCancelledForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                OccurredAt: DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error cancelling order {OrderId}", cmd.OrderId);

            // Still publish completion - compensation should not fail the saga
            await context.Publish(new OrderCancelledForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                OccurredAt: DateTime.UtcNow));
        }
    }
}

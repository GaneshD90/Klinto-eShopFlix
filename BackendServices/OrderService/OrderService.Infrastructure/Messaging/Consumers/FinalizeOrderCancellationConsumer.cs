using Contracts.Sagas.Cancellation;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles FinalizeOrderCancellation command.
/// Updates order status to cancelled.
/// </summary>
public sealed class FinalizeOrderCancellationConsumer : IConsumer<FinalizeOrderCancellation>
{
    private readonly OrderServiceDbContext _dbContext;
    private readonly ILogger<FinalizeOrderCancellationConsumer> _logger;

    public FinalizeOrderCancellationConsumer(
        OrderServiceDbContext dbContext,
        ILogger<FinalizeOrderCancellationConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FinalizeOrderCancellation> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "FinalizeOrderCancellationConsumer processing for OrderId={OrderId}, CorrelationId={CorrelationId}",
            cmd.OrderId, cmd.CorrelationId);

        try
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.OrderId == cmd.OrderId, context.CancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for cancellation finalization", cmd.OrderId);
            }
            else
            {
                order.OrderStatus = "Cancelled";
                order.CancelledAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                order.CustomerNotes = string.IsNullOrEmpty(order.CustomerNotes)
                    ? $"[{cmd.CancellationType}] {cmd.CancellationReason}"
                    : $"{order.CustomerNotes}\n[{cmd.CancellationType}] {cmd.CancellationReason}";

                await _dbContext.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation(
                    "Order {OrderId} cancellation finalized",
                    cmd.OrderId);
            }

            await context.Publish(new OrderCancellationFinalized(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                OrderNumber: cmd.OrderNumber,
                CancelledAt: DateTime.UtcNow,
                OccurredAt: DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error finalizing cancellation for OrderId={OrderId}", cmd.OrderId);

            // Still publish completion - saga should not fail at final step
            await context.Publish(new OrderCancellationFinalized(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                OrderNumber: cmd.OrderNumber,
                CancelledAt: DateTime.UtcNow,
                OccurredAt: DateTime.UtcNow));
        }
    }
}

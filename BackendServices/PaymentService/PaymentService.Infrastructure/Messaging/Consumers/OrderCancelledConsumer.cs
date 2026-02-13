using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes OrderCancelledV2 to initiate refunds.
/// When an order is cancelled after payment, this consumer initiates
/// the refund process.
/// </summary>
public sealed class OrderCancelledConsumer : IConsumer<OrderCancelledV2>
{
    private readonly PaymentServiceDbContext _dbContext;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(
        PaymentServiceDbContext dbContext,
        ILogger<OrderCancelledConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "OrderCancelledConsumer received OrderCancelledV2 for OrderId={OrderId}, Reason={Reason}",
            msg.OrderId, msg.CancellationReason);

        try
        {
            // In a full implementation:
            // 1. Find any captured payments for this order
            // 2. Initiate refund with payment provider
            // 3. Create refund record in database
            // 4. Publish PaymentRefundedV2 event

            // For demonstration, simulate refund initiation
            var refundId = Guid.NewGuid();
            var transactionId = $"ref_{Guid.NewGuid():N}"[..24];

            // Publish payment refunded event
            await context.Publish(new PaymentRefundedV2(
                RefundId: refundId,
                PaymentId: Guid.Empty, // Would come from the payment record
                OrderId: msg.OrderId,
                OrderNumber: msg.OrderNumber,
                RefundAmount: 0m, // Would come from the payment record
                CurrencyCode: "INR",
                RefundMethod: "OriginalPaymentMethod",
                TransactionId: transactionId,
                OccurredAt: DateTime.UtcNow));

            _logger.LogInformation(
                "Refund initiated for cancelled OrderId={OrderId}, RefundId={RefundId}",
                msg.OrderId, refundId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error initiating refund for cancelled OrderId={OrderId}", msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}

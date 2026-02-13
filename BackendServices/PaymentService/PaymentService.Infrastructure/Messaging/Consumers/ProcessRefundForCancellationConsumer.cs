using Contracts.Sagas.Cancellation;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles ProcessRefundForCancellation command.
/// Processes refund for a cancelled order.
/// </summary>
public sealed class ProcessRefundForCancellationConsumer : IConsumer<ProcessRefundForCancellation>
{
    private readonly PaymentServiceDbContext _dbContext;
    private readonly ILogger<ProcessRefundForCancellationConsumer> _logger;

    public ProcessRefundForCancellationConsumer(
        PaymentServiceDbContext dbContext,
        ILogger<ProcessRefundForCancellationConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessRefundForCancellation> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "ProcessRefundForCancellationConsumer processing for OrderId={OrderId}, Amount={Amount} {Currency}",
            cmd.OrderId, cmd.RefundAmount, cmd.CurrencyCode);

        try
        {
            // In production: Call payment provider API to process refund
            // var refundResult = await _paymentProvider.RefundAsync(cmd.PaymentId, cmd.RefundAmount);

            var refundId = Guid.NewGuid();
            var transactionId = $"ref_{Guid.NewGuid():N}"[..24];

            // Simulate successful refund
            _logger.LogInformation(
                "Refund processed for OrderId={OrderId}, RefundId={RefundId}, Amount={Amount}",
                cmd.OrderId, refundId, cmd.RefundAmount);

            await context.Publish(new RefundProcessedForCancellation(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                RefundId: refundId,
                TransactionId: transactionId,
                RefundAmount: cmd.RefundAmount,
                OccurredAt: DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing refund for OrderId={OrderId}", cmd.OrderId);

            await context.Publish(new RefundFailedForCancellation(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                Reason: $"Refund processing error: {ex.Message}",
                ErrorCode: "REFUND_ERROR",
                OccurredAt: DateTime.UtcNow));
        }
    }
}

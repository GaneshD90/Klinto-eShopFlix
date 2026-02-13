using Contracts.Sagas.ReturnRefund;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles ProcessReturnRefund command.
/// Processes refund for returned items.
/// </summary>
public sealed class ProcessReturnRefundConsumer : IConsumer<ProcessReturnRefund>
{
    private readonly PaymentServiceDbContext _dbContext;
    private readonly ILogger<ProcessReturnRefundConsumer> _logger;

    public ProcessReturnRefundConsumer(
        PaymentServiceDbContext dbContext,
        ILogger<ProcessReturnRefundConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessReturnRefund> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "ProcessReturnRefundConsumer processing for OrderId={OrderId}, Amount={Amount} {Currency}, Method={Method}",
            cmd.OrderId, cmd.RefundAmount, cmd.CurrencyCode, cmd.RefundMethod);

        try
        {
            var refundId = Guid.NewGuid();
            var transactionId = $"rtn_{Guid.NewGuid():N}"[..24];

            // In production: Process based on refund method
            switch (cmd.RefundMethod)
            {
                case "OriginalPayment":
                    // Call payment provider to refund to original payment method
                    _logger.LogInformation("Processing refund to original payment method");
                    break;

                case "StoreCredit":
                    // Add store credit to customer account
                    _logger.LogInformation("Processing store credit for customer {CustomerId}", cmd.CustomerId);
                    break;

                case "BankTransfer":
                    // Process bank transfer (requires additional info)
                    _logger.LogInformation("Processing bank transfer refund");
                    break;

                default:
                    _logger.LogInformation("Processing refund with default method");
                    break;
            }

            _logger.LogInformation(
                "Return refund processed for OrderId={OrderId}, RefundId={RefundId}",
                cmd.OrderId, refundId);

            await context.Publish(new ReturnRefundProcessed(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ReturnId: null, // Would come from saga state
                RefundId: refundId,
                TransactionId: transactionId,
                RefundAmount: cmd.RefundAmount,
                RefundMethod: cmd.RefundMethod,
                OccurredAt: DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing return refund for OrderId={OrderId}", cmd.OrderId);

            await context.Publish(new ReturnRefundProcessingFailed(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ReturnId: null,
                Reason: $"Return refund error: {ex.Message}",
                ErrorCode: "RETURN_REFUND_ERROR",
                OccurredAt: DateTime.UtcNow));
        }
    }
}

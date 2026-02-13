using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes OrderConfirmedV2 to capture previously authorized payments.
/// When an order is confirmed, this consumer captures the payment that was
/// authorized during checkout.
/// </summary>
public sealed class OrderConfirmedConsumer : IConsumer<OrderConfirmedV2>
{
    private readonly PaymentServiceDbContext _dbContext;
    private readonly ILogger<OrderConfirmedConsumer> _logger;

    public OrderConfirmedConsumer(
        PaymentServiceDbContext dbContext,
        ILogger<OrderConfirmedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderConfirmedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "OrderConfirmedConsumer received OrderConfirmedV2 for OrderId={OrderId}, OrderNumber={OrderNumber}",
            msg.OrderId, msg.OrderNumber);

        try
        {
            // In a full implementation:
            // 1. Find the authorized payment for this order
            // 2. Call the payment provider to capture the payment
            // 3. Update the payment status to "Captured"
            // 4. Publish PaymentCapturedV2 event

            // For demonstration, simulate capture
            var captureId = Guid.NewGuid();
            var transactionId = $"cap_{Guid.NewGuid():N}"[..24];

            // Publish payment captured event
            await context.Publish(new PaymentCapturedV2(
                PaymentId: captureId,
                OrderId: msg.OrderId,
                OrderNumber: msg.OrderNumber,
                Amount: 0m, // Would come from the authorized payment record
                CurrencyCode: "INR",
                TransactionId: transactionId,
                Status: "Captured",
                OccurredAt: DateTime.UtcNow));

            _logger.LogInformation(
                "Payment captured for OrderId={OrderId}, TransactionId={TransactionId}",
                msg.OrderId, transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error capturing payment for OrderId={OrderId}", msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}

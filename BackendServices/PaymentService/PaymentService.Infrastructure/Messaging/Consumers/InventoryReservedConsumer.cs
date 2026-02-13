using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Infrastructure.Providers.Abstractions;

namespace PaymentService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes InventoryReservedV2 as part of the order saga.
/// After stock is reserved, attempts to authorize payment.
/// Publishes PaymentAuthorizedV2 on success, or PaymentFailedV2 on failure.
/// 
/// Note: In production, this consumer would:
/// 1. Fetch order details (amount, customer payment method) from OrderService or a local cache
/// 2. Call the actual payment provider (RazorPay) to authorize
/// 3. Store the payment record in PaymentService database
/// 
/// For saga demonstration, we simulate authorization.
/// </summary>
public sealed class InventoryReservedConsumer : IConsumer<InventoryReservedV2>
{
    private readonly ILogger<InventoryReservedConsumer> _logger;
    // Uncomment when ready to use real payment provider:
    // private readonly IPaymentProvider _paymentProvider;

    public InventoryReservedConsumer(
        ILogger<InventoryReservedConsumer> logger
        // IPaymentProvider paymentProvider
        )
    {
        _logger = logger;
        // _paymentProvider = paymentProvider;
    }

    public async Task Consume(ConsumeContext<InventoryReservedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "InventoryReservedConsumer received InventoryReservedV2 for OrderId={OrderId}, ReservationId={ReservationId}, Quantity={Quantity}",
            msg.OrderId, msg.ReservationId, msg.ReservedQuantity);

        try
        {
            // In production scenario:
            // 1. Fetch order from OrderService to get payment details
            // 2. Call payment provider: var result = await _paymentProvider.AuthorizeAsync(order.Amount, order.PaymentMethodId);
            // 3. Store payment record
            
            // For demonstration, simulate payment authorization
            var paymentId = Guid.NewGuid();
            var transactionId = $"txn_{Guid.NewGuid():N}"[..24];
            var authorizationCode = $"auth_{Guid.NewGuid():N}"[..16];

            // Simulate occasional payment failures (for testing compensation flow)
            // Remove this in production
            var random = new Random();
            if (random.Next(100) < 5) // 5% failure rate for testing
            {
                throw new InvalidOperationException("Simulated payment authorization failure");
            }

            // Publish successful authorization
            await context.Publish(new PaymentAuthorizedV2(
                PaymentId: paymentId,
                OrderId: msg.OrderId,
                OrderNumber: string.Empty, // Will be enriched by OrderService
                Amount: 0m, // In production: order.TotalAmount
                CurrencyCode: "INR",
                PaymentMethod: "RazorPay",
                PaymentProvider: "RazorPay",
                TransactionId: transactionId,
                AuthorizationCode: authorizationCode,
                Status: "Authorized",
                OccurredAt: DateTime.UtcNow));

            _logger.LogInformation(
                "Payment authorized for OrderId={OrderId}, PaymentId={PaymentId}, TransactionId={TransactionId}",
                msg.OrderId, paymentId, transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Payment authorization failed for OrderId={OrderId}: {Message}",
                msg.OrderId, ex.Message);

            // Publish payment failure - triggers compensation flow
            await context.Publish(new PaymentFailedV2(
                PaymentId: Guid.NewGuid(),
                OrderId: msg.OrderId,
                OrderNumber: string.Empty,
                Reason: ex.Message,
                ErrorCode: "PAYMENT_AUTH_FAILED",
                PaymentMethod: "RazorPay",
                OccurredAt: DateTime.UtcNow));
        }
    }
}

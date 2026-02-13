using Contracts.Sagas.Checkout;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles AuthorizePaymentForCheckout command.
/// Called by the Checkout Saga orchestrator to authorize payment after inventory is reserved.
/// </summary>
public sealed class AuthorizePaymentForCheckoutConsumer : IConsumer<AuthorizePaymentForCheckout>
{
    private readonly PaymentServiceDbContext _dbContext;
    private readonly ILogger<AuthorizePaymentForCheckoutConsumer> _logger;

    public AuthorizePaymentForCheckoutConsumer(
        PaymentServiceDbContext dbContext,
        ILogger<AuthorizePaymentForCheckoutConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuthorizePaymentForCheckout> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "AuthorizePaymentForCheckoutConsumer processing command for OrderId={OrderId}, CorrelationId={CorrelationId}, Amount={Amount} {Currency}",
            cmd.OrderId, cmd.CorrelationId, cmd.Amount, cmd.CurrencyCode);

        try
        {
            // Simulate payment authorization
            // In production, this would call the actual payment provider (Razorpay, Stripe, etc.)
            var paymentId = Guid.NewGuid();
            var transactionId = $"auth_{Guid.NewGuid():N}"[..24];
            var success = true;
            var failureReason = string.Empty;

            // Simulate random failures for testing (10% failure rate)
            // Remove this in production!
            var random = new Random();
            if (random.Next(100) < 10)
            {
                success = false;
                failureReason = "Payment declined by provider";
            }

            // Validate amount
            if (cmd.Amount <= 0)
            {
                success = false;
                failureReason = "Invalid payment amount";
            }

            if (success)
            {
                // In production: Save payment record to database
                // var payment = new PaymentTransaction { ... };
                // _dbContext.PaymentTransactions.Add(payment);
                // await _dbContext.SaveChangesAsync(context.CancellationToken);

                // Publish success event
                await context.Publish(new PaymentAuthorizedForCheckout(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    PaymentId: paymentId,
                    TransactionId: transactionId,
                    Amount: cmd.Amount,
                    OccurredAt: DateTime.UtcNow));

                _logger.LogInformation(
                    "Payment authorized for OrderId={OrderId}, PaymentId={PaymentId}, TransactionId={TransactionId}",
                    cmd.OrderId, paymentId, transactionId);
            }
            else
            {
                // Publish failure event
                await context.Publish(new PaymentFailedForCheckout(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    Reason: failureReason,
                    ErrorCode: "PAYMENT_DECLINED",
                    OccurredAt: DateTime.UtcNow));

                _logger.LogWarning(
                    "Payment failed for OrderId={OrderId}: {Reason}",
                    cmd.OrderId, failureReason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing payment for OrderId={OrderId}", cmd.OrderId);

            // Publish failure event
            await context.Publish(new PaymentFailedForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                Reason: $"System error: {ex.Message}",
                ErrorCode: "SYSTEM_ERROR",
                OccurredAt: DateTime.UtcNow));
        }
    }
}

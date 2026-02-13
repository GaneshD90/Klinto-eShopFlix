using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Application.CQRS;
using PaymentService.Application.Payments.Commands;
using Contracts.Events.V2;

namespace PaymentService.Application.Payments.Handlers;

/// <summary>
/// Handler for AuthorizePaymentCommand.
/// Authorizes payment and publishes PaymentAuthorizedV2 or PaymentFailedV2 event.
/// </summary>
public sealed class AuthorizePaymentCommandHandler : ICommandHandler<AuthorizePaymentCommand, PaymentAuthorizationResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthorizePaymentCommandHandler> _logger;

    public AuthorizePaymentCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<AuthorizePaymentCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<PaymentAuthorizationResult> Handle(AuthorizePaymentCommand command, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Authorizing payment for OrderId={OrderId}, Amount={Amount} {Currency}",
            command.OrderId, command.Amount, command.CurrencyCode);

        try
        {
            // In production: Call actual payment provider (Razorpay, Stripe, etc.)
            // var result = await _razorpayClient.AuthorizeAsync(command);

            // Simulate payment authorization
            var paymentId = Guid.NewGuid();
            var transactionId = $"auth_{Guid.NewGuid():N}"[..24];
            var success = true;
            var errorMessage = string.Empty;

            // Simulate validation
            if (command.Amount <= 0)
            {
                success = false;
                errorMessage = "Invalid payment amount";
            }

            if (success)
            {
                // Publish success event
                await _publishEndpoint.Publish(new PaymentAuthorizedV2(
                    PaymentId: paymentId,
                    OrderId: command.OrderId,
                    OrderNumber: command.OrderNumber,
                    Amount: command.Amount,
                    CurrencyCode: command.CurrencyCode,
                    PaymentMethod: command.PaymentMethod ?? "Card",
                    PaymentProvider: "Razorpay",
                    TransactionId: transactionId,
                    AuthorizationCode: $"AUTH{Guid.NewGuid():N}"[..10],
                    Status: "Authorized",
                    OccurredAt: DateTime.UtcNow), ct);

                _logger.LogInformation(
                    "Payment authorized: PaymentId={PaymentId}, TransactionId={TransactionId}",
                    paymentId, transactionId);

                return new PaymentAuthorizationResult(true, paymentId, transactionId, null, null);
            }
            else
            {
                // Publish failure event
                await _publishEndpoint.Publish(new PaymentFailedV2(
                    PaymentId: Guid.Empty,
                    OrderId: command.OrderId,
                    OrderNumber: command.OrderNumber,
                    Reason: errorMessage,
                    ErrorCode: "VALIDATION_ERROR",
                    PaymentMethod: command.PaymentMethod ?? "Card",
                    OccurredAt: DateTime.UtcNow), ct);

                _logger.LogWarning(
                    "Payment authorization failed for OrderId={OrderId}: {Reason}",
                    command.OrderId, errorMessage);

                return new PaymentAuthorizationResult(false, null, null, errorMessage, "VALIDATION_ERROR");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authorizing payment for OrderId={OrderId}", command.OrderId);

            await _publishEndpoint.Publish(new PaymentFailedV2(
                PaymentId: Guid.Empty,
                OrderId: command.OrderId,
                OrderNumber: command.OrderNumber,
                Reason: ex.Message,
                ErrorCode: "SYSTEM_ERROR",
                PaymentMethod: command.PaymentMethod ?? "Card",
                OccurredAt: DateTime.UtcNow), ct);

            return new PaymentAuthorizationResult(false, null, null, ex.Message, "SYSTEM_ERROR");
        }
    }
}

/// <summary>
/// Handler for CapturePaymentCommand.
/// Captures an authorized payment.
/// </summary>
public sealed class CapturePaymentCommandHandler : ICommandHandler<CapturePaymentCommand, PaymentCaptureResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CapturePaymentCommandHandler> _logger;

    public CapturePaymentCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<CapturePaymentCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<PaymentCaptureResult> Handle(CapturePaymentCommand command, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Capturing payment PaymentId={PaymentId} for OrderId={OrderId}",
            command.PaymentId, command.OrderId);

        try
        {
            // In production: Call payment provider to capture
            var transactionId = $"cap_{Guid.NewGuid():N}"[..24];

            await _publishEndpoint.Publish(new PaymentCapturedV2(
                PaymentId: command.PaymentId,
                OrderId: command.OrderId,
                OrderNumber: string.Empty,
                Amount: command.Amount,
                CurrencyCode: "INR",
                TransactionId: transactionId,
                Status: "Captured",
                OccurredAt: DateTime.UtcNow), ct);

            _logger.LogInformation(
                "Payment captured: PaymentId={PaymentId}, TransactionId={TransactionId}",
                command.PaymentId, transactionId);

            return new PaymentCaptureResult(true, transactionId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing payment PaymentId={PaymentId}", command.PaymentId);
            return new PaymentCaptureResult(false, null, ex.Message);
        }
    }
}

/// <summary>
/// Handler for ProcessRefundCommand.
/// Processes refund and publishes PaymentRefundedV2 event.
/// </summary>
public sealed class ProcessRefundCommandHandler : ICommandHandler<ProcessRefundCommand, RefundResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessRefundCommandHandler> _logger;

    public ProcessRefundCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<ProcessRefundCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<RefundResult> Handle(ProcessRefundCommand command, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Processing refund for PaymentId={PaymentId}, Amount={Amount}",
            command.PaymentId, command.RefundAmount);

        try
        {
            // In production: Call payment provider to process refund
            var refundId = Guid.NewGuid();
            var transactionId = $"ref_{Guid.NewGuid():N}"[..24];

            await _publishEndpoint.Publish(new PaymentRefundedV2(
                RefundId: refundId,
                PaymentId: command.PaymentId,
                OrderId: command.OrderId,
                OrderNumber: string.Empty,
                RefundAmount: command.RefundAmount,
                CurrencyCode: "INR",
                RefundMethod: command.RefundMethod,
                TransactionId: transactionId,
                OccurredAt: DateTime.UtcNow), ct);

            _logger.LogInformation(
                "Refund processed: RefundId={RefundId}, Amount={Amount}",
                refundId, command.RefundAmount);

            return new RefundResult(true, refundId, transactionId, command.RefundAmount, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for PaymentId={PaymentId}", command.PaymentId);
            return new RefundResult(false, null, null, 0, ex.Message);
        }
    }
}

using PaymentService.Application.CQRS;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Payments.Commands;

/// <summary>
/// Command to create a new payment order (e.g., Razorpay order).
/// </summary>
public record CreatePaymentOrderCommand(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    decimal Amount,
    string CurrencyCode,
    string PaymentMethod
) : ICommand<PaymentOrderResult>;

/// <summary>
/// Command to authorize a payment.
/// </summary>
public record AuthorizePaymentCommand(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    decimal Amount,
    string CurrencyCode,
    string PaymentMethod,
    string? PaymentToken
) : ICommand<PaymentAuthorizationResult>;

/// <summary>
/// Command to capture an authorized payment.
/// </summary>
public record CapturePaymentCommand(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount
) : ICommand<PaymentCaptureResult>;

/// <summary>
/// Command to confirm a payment (e.g., after Razorpay callback).
/// </summary>
public record ConfirmPaymentCommand(
    string RazorpayOrderId,
    string RazorpayPaymentId,
    string RazorpaySignature,
    Guid OrderId
) : ICommand<PaymentConfirmResult>;

/// <summary>
/// Command to process a refund.
/// </summary>
public record ProcessRefundCommand(
    Guid PaymentId,
    Guid OrderId,
    decimal RefundAmount,
    string Reason,
    string RefundMethod
) : ICommand<RefundResult>;

// ============ Result DTOs ============

public record PaymentOrderResult(
    bool Success,
    Guid? PaymentId,
    string? ProviderOrderId,
    string? ErrorMessage
);

public record PaymentAuthorizationResult(
    bool Success,
    Guid? PaymentId,
    string? TransactionId,
    string? ErrorMessage,
    string? ErrorCode
);

public record PaymentCaptureResult(
    bool Success,
    string? TransactionId,
    string? ErrorMessage
);

public record PaymentConfirmResult(
    bool Success,
    Guid? PaymentId,
    string? TransactionId,
    string? ErrorMessage
);

public record RefundResult(
    bool Success,
    Guid? RefundId,
    string? TransactionId,
    decimal RefundedAmount,
    string? ErrorMessage
);

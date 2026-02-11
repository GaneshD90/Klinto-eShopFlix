using Contracts.DTOs;

namespace Contracts.Events.V2
{
    // ============ Order Lifecycle ============

    /// <summary>Raised when a new order is created (direct or from cart).</summary>
    public record OrderCreatedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        string OrderType,
        string OrderSource,
        decimal TotalAmount,
        string CurrencyCode,
        int ItemCount,
        Guid? CartId,
        IEnumerable<OrderLineV2Dto> Lines,
        DateTime OccurredAt
    );

    /// <summary>Raised when an order is confirmed and ready for fulfillment.</summary>
    public record OrderConfirmedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        DateTime ConfirmedAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when an order is cancelled with full context.</summary>
    public record OrderCancelledV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        string CancellationType,
        string CancellationReason,
        Guid? CancelledBy,
        DateTime CancelledAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when order status changes (generic).</summary>
    public record OrderStatusChangedV2(
        Guid OrderId,
        string OrderNumber,
        string FromStatus,
        string ToStatus,
        Guid? ChangedBy,
        string? Reason,
        bool NotifyCustomer,
        DateTime ChangedAt,
        DateTime OccurredAt
    );

    // ============ Payments ============

    /// <summary>Raised when a payment is processed against an order.</summary>
    public record OrderPaymentProcessedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? PaymentId,
        string PaymentMethod,
        string PaymentProvider,
        decimal Amount,
        string CurrencyCode,
        string TransactionId,
        string Status,
        DateTime ProcessedAt,
        DateTime OccurredAt
    );

    // ============ Fulfillment & Shipping ============

    /// <summary>Raised when a shipment is created for an order.</summary>
    public record ShipmentCreatedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        Guid? ShipmentId,
        string ShipmentNumber,
        string ShippingMethod,
        string CarrierName,
        Guid? WarehouseId,
        DateTime CreatedAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when an order is shipped (tracking available).</summary>
    public record OrderShippedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid ShipmentId,
        string TrackingNumber,
        string? TrackingUrl,
        string CarrierName,
        DateTime? EstimatedDeliveryDate,
        DateTime ShippedAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when an order is delivered.</summary>
    public record OrderDeliveredV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid ShipmentId,
        string? DeliverySignature,
        DateTime DeliveredAt,
        DateTime OccurredAt
    );

    // ============ Returns & Refunds ============

    /// <summary>Raised when a customer requests a return.</summary>
    public record ReturnRequestedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? ReturnId,
        string ReturnNumber,
        string ReturnType,
        string ReturnReason,
        string? CustomerComments,
        DateTime RequestedAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when a refund is processed.</summary>
    public record RefundProcessedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? RefundId,
        string RefundNumber,
        decimal RefundAmount,
        string CurrencyCode,
        string RefundType,
        string RefundMethod,
        string RefundReason,
        DateTime ProcessedAt,
        DateTime OccurredAt
    );

    // ============ Fraud & Holds ============

    /// <summary>Raised when an order is placed on hold.</summary>
    public record OrderPlacedOnHoldV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string HoldType,
        string HoldReason,
        Guid? PlacedBy,
        DateTime? ExpiresAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when an order hold is released.</summary>
    public record OrderHoldReleasedV2(
        Guid OrderId,
        string OrderNumber,
        Guid HoldId,
        Guid? ReleasedBy,
        DateTime ReleasedAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when a fraud check is performed.</summary>
    public record FraudCheckPerformedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string FraudProvider,
        string RiskLevel,
        decimal? RiskScore,
        string Status,
        DateTime CheckedAt,
        DateTime OccurredAt
    );

    // ============ Subscriptions ============

    /// <summary>Raised when a subscription is created from an order.</summary>
    public record SubscriptionCreatedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? SubscriptionId,
        string Frequency,
        DateTime? StartDate,
        int? TotalOccurrences,
        DateTime CreatedAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when a subscription's next order is processed.</summary>
    public record SubscriptionOrderProcessedV2(
        Guid SubscriptionId,
        string Status,
        string Message,
        DateTime ProcessedAt,
        DateTime OccurredAt
    );

    // ============ Order Items ============

    /// <summary>Raised when items are added to an existing order.</summary>
    public record OrderItemsAddedV2(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        DateTime AddedAt,
        DateTime OccurredAt
    );
}

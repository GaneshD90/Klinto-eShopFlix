namespace eShopFlix.Web.Models.Order;

// ============ Core Order Models ============

public class OrderDetailModel
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string FulfillmentStatus { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string OrderSource { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public List<OrderItemModel> Items { get; set; } = [];
    public List<OrderAddressModel> Addresses { get; set; } = [];

    public string StatusBadgeClass => OrderStatus switch
    {
        "Completed" or "Delivered" => "bg-success",
        "Cancelled" => "bg-danger",
        "OnHold" => "bg-warning text-dark",
        "Processing" or "Confirmed" => "bg-primary",
        "Shipped" => "bg-info text-dark",
        _ => "bg-secondary"
    };
}

public class OrderListItemModel
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string FulfillmentStatus { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int ItemCount { get; set; }

    public string StatusBadgeClass => OrderStatus switch
    {
        "Completed" or "Delivered" => "bg-success",
        "Cancelled" => "bg-danger",
        "OnHold" => "bg-warning text-dark",
        "Processing" or "Confirmed" => "bg-primary",
        "Shipped" => "bg-info text-dark",
        _ => "bg-secondary"
    };
}

public class OrderItemModel
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariationId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string ItemStatus { get; set; } = string.Empty;
    public bool IsGift { get; set; }
    public string? GiftMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderAddressModel
{
    public Guid OrderAddressId { get; set; }
    public string AddressType { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}

// ============ Paged Result ============

public class PagedResultModel<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

// ============ Status & Timeline ============

public class OrderStatusHistoryModel
{
    public Guid HistoryId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public Guid? ChangedBy { get; set; }
    public string? ChangeReason { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class OrderTimelineModel
{
    public Guid TimelineId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public bool IsCustomerVisible { get; set; }
}

public class OrderNoteModel
{
    public Guid NoteId { get; set; }
    public string NoteType { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsVisibleToCustomer { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============ Payments ============

public class OrderPaymentModel
{
    public Guid PaymentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentProvider { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public string StatusBadgeClass => PaymentStatus switch
    {
        "Completed" or "Paid" => "bg-success",
        "Failed" => "bg-danger",
        "Pending" => "bg-warning text-dark",
        "Refunded" => "bg-info text-dark",
        _ => "bg-secondary"
    };
}

// ============ Shipments ============

public class OrderShipmentModel
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public string ShipmentStatus { get; set; } = string.Empty;
    public string? ShippingMethod { get; set; }
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public List<OrderShipmentItemModel> Items { get; set; } = [];
}

public class OrderShipmentItemModel
{
    public Guid ShipmentItemId { get; set; }
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
}

public class ShipmentTrackingModel
{
    public Guid ShipmentId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string ShipmentStatus { get; set; } = string.Empty;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}

// ============ Returns & Refunds ============

public class OrderReturnModel
{
    public Guid ReturnId { get; set; }
    public Guid OrderId { get; set; }
    public string ReturnStatus { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public string ReturnReason { get; set; } = string.Empty;
    public string? CustomerComments { get; set; }
    public DateTime RequestedAt { get; set; }
    public List<OrderReturnItemModel> Items { get; set; } = [];
}

public class OrderReturnItemModel
{
    public Guid ReturnItemId { get; set; }
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public string? Condition { get; set; }
}

public class OrderRefundModel
{
    public Guid RefundId { get; set; }
    public Guid OrderId { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundType { get; set; } = string.Empty;
    public string RefundMethod { get; set; } = string.Empty;
    public string RefundStatus { get; set; } = string.Empty;
    public string? RefundReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============ Command Result Models ============

public class CommandResultModel
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess => string.Equals(Status, "Success", StringComparison.OrdinalIgnoreCase);
}

public class CreateOrderFromCartResultModel : CommandResultModel
{
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
}

public class ConfirmOrderResultModel : CommandResultModel
{
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
}

public class ProcessPaymentResultModel : CommandResultModel
{
    public Guid? PaymentId { get; set; }
}

// ============ Analytics Models ============

public class CustomerOrderHistoryModel
{
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public int? TotalOrders { get; set; }
    public decimal? TotalSpent { get; set; }
    public decimal? AverageOrderValue { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public DateTime? FirstOrderDate { get; set; }
    public int? CompletedOrders { get; set; }
    public int? CancelledOrders { get; set; }
    public string CustomerSegment { get; set; } = string.Empty;
}

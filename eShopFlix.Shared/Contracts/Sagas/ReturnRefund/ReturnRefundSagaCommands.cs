namespace Contracts.Sagas.ReturnRefund;

/// <summary>
/// Commands sent by the return/refund saga orchestrator to participant services.
/// </summary>

/// <summary>
/// Command to validate a return request.
/// </summary>
public record ValidateReturnRequest(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid? ReturnId,
    string ReturnType,
    string ReturnReason,
    IEnumerable<ReturnLineItem> Items
);

/// <summary>
/// Command to process return items back into inventory.
/// </summary>
public record RestockReturnedItems(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid? ReturnId,
    IEnumerable<ReturnLineItem> Items,
    Guid? PreferredWarehouseId
);

/// <summary>
/// Command to process refund for returned items.
/// </summary>
public record ProcessReturnRefund(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    Guid? PaymentId,
    decimal RefundAmount,
    string CurrencyCode,
    string RefundMethod, // OriginalPayment, StoreCredit, BankTransfer
    string RefundReason
);

/// <summary>
/// Command to finalize return status.
/// </summary>
public record FinalizeReturn(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid? ReturnId,
    string ReturnNumber,
    string FinalStatus
);

/// <summary>
/// Command to notify customer about return status.
/// </summary>
public record NotifyCustomerOfReturnStatus(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string CustomerEmail,
    string ReturnNumber,
    string Status,
    decimal? RefundAmount,
    string? RefundMethod
);

/// <summary>
/// Line item for return processing.
/// </summary>
public record ReturnLineItem(
    Guid OrderItemId,
    Guid ProductId,
    Guid? VariationId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    string? ReturnCondition // Good, Damaged, Defective
);

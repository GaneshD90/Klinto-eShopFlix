namespace Contracts.Sagas.ReturnRefund;

/// <summary>
/// State data persisted for each return/refund saga instance.
/// Tracks the return through validation, restocking, and refund processing.
/// </summary>
public class ReturnRefundSagaState : MassTransit.SagaStateMachineInstance
{
    /// <summary>
    /// Unique correlation ID for this saga instance.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Current state of the saga.
    /// </summary>
    public string CurrentState { get; set; } = string.Empty;

    // ============ Order Context ============

    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;

    // ============ Return Context ============

    public Guid? ReturnId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty; // Refund, Exchange, StoreCredit
    public string ReturnReason { get; set; } = string.Empty;
    public string? CustomerComments { get; set; }
    public int TotalItemsToReturn { get; set; }

    // ============ Saga Progress ============

    public DateTime? RequestedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? RestockedAt { get; set; }
    public DateTime? RefundProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // ============ Validation Context ============

    public bool IsValidated { get; set; }
    public string? ValidationNotes { get; set; }
    public bool RequiresInspection { get; set; }

    // ============ Restock Context ============

    public int ItemsRestocked { get; set; }
    public Guid? WarehouseId { get; set; }

    // ============ Refund Context ============

    public Guid? PaymentId { get; set; }
    public Guid? RefundId { get; set; }
    public decimal RefundAmount { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string? RefundTransactionId { get; set; }
    public string? RefundMethod { get; set; } // OriginalPayment, StoreCredit, BankTransfer

    // ============ Failure Context ============

    public string? FailureReason { get; set; }
    public string? FailedStep { get; set; }

    // ============ Concurrency ============

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

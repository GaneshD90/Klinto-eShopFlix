namespace Contracts.Sagas.Checkout;

/// <summary>
/// State data persisted for each checkout saga instance.
/// Tracks the order through the checkout process and stores
/// data needed for compensation if failures occur.
/// </summary>
public class CheckoutSagaState : MassTransit.SagaStateMachineInstance
{
    /// <summary>
    /// Unique correlation ID for this saga instance (same as OrderId).
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Current state of the saga (e.g., "AwaitingInventory", "AwaitingPayment", "Completed").
    /// </summary>
    public string CurrentState { get; set; } = string.Empty;

    // ============ Order Context ============

    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public int ItemCount { get; set; }
    public Guid? CartId { get; set; }

    // ============ Saga Progress ============

    /// <summary>Timestamp when the saga was initiated.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Timestamp when inventory was reserved.</summary>
    public DateTime? InventoryReservedAt { get; set; }

    /// <summary>Primary reservation ID from StockService.</summary>
    public Guid? ReservationId { get; set; }

    /// <summary>Timestamp when payment was authorized.</summary>
    public DateTime? PaymentAuthorizedAt { get; set; }

    /// <summary>Payment ID from PaymentService.</summary>
    public Guid? PaymentId { get; set; }

    /// <summary>Payment transaction ID from provider.</summary>
    public string? TransactionId { get; set; }

    /// <summary>Timestamp when order was confirmed.</summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>Timestamp when saga completed (success or failure).</summary>
    public DateTime? CompletedAt { get; set; }

    // ============ Failure Context ============

    /// <summary>If saga failed, the reason for failure.</summary>
    public string? FailureReason { get; set; }

    /// <summary>If saga failed, the step that failed.</summary>
    public string? FailedStep { get; set; }

    /// <summary>Number of compensation steps executed.</summary>
    public int CompensationStepsExecuted { get; set; }

    // ============ Concurrency ============

    /// <summary>Row version for optimistic concurrency in EF Core.</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

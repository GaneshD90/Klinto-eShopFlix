namespace Contracts.Sagas.Cancellation;

/// <summary>
/// State data persisted for each order cancellation saga instance.
/// Tracks the cancellation through releasing stock and processing refunds.
/// </summary>
public class CancellationSagaState : MassTransit.SagaStateMachineInstance
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
    public decimal OrderAmount { get; set; }
    public string CurrencyCode { get; set; } = "INR";

    // ============ Cancellation Context ============

    public string CancellationType { get; set; } = string.Empty; // Customer, Admin, System
    public string CancellationReason { get; set; } = string.Empty;
    public Guid? CancelledBy { get; set; }

    // ============ Saga Progress ============

    public DateTime? RequestedAt { get; set; }
    public DateTime? StockReleasedAt { get; set; }
    public DateTime? RefundInitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // ============ Payment Context ============

    public Guid? PaymentId { get; set; }
    public Guid? RefundId { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundTransactionId { get; set; }

    // ============ Failure Context ============

    public string? FailureReason { get; set; }
    public string? FailedStep { get; set; }

    // ============ Concurrency ============

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

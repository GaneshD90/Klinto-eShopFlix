using OrderService.Application.Sagas.DTOs;

namespace OrderService.Application.Sagas.Services;

/// <summary>
/// Interface for querying saga state for monitoring and dashboards.
/// </summary>
public interface ISagaMonitoringService
{
    /// <summary>
    /// Gets a summary of all sagas with optional filtering.
    /// </summary>
    Task<IEnumerable<SagaStateSummaryDto>> GetSagaSummariesAsync(
        string? sagaType = null,
        string? state = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default);

    /// <summary>
    /// Gets checkout saga details by correlation ID.
    /// </summary>
    Task<CheckoutSagaDetailsDto?> GetCheckoutSagaAsync(Guid correlationId, CancellationToken ct = default);

    /// <summary>
    /// Gets cancellation saga details by correlation ID.
    /// </summary>
    Task<CancellationSagaDetailsDto?> GetCancellationSagaAsync(Guid correlationId, CancellationToken ct = default);

    /// <summary>
    /// Gets return/refund saga details by correlation ID.
    /// </summary>
    Task<ReturnRefundSagaDetailsDto?> GetReturnRefundSagaAsync(Guid correlationId, CancellationToken ct = default);

    /// <summary>
    /// Gets sagas by order ID.
    /// </summary>
    Task<IEnumerable<SagaStateSummaryDto>> GetSagasByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Gets saga statistics.
    /// </summary>
    Task<IEnumerable<SagaStatisticsDto>> GetSagaStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets count of sagas in each state.
    /// </summary>
    Task<Dictionary<string, int>> GetSagaStateCountsAsync(string sagaType, CancellationToken ct = default);
}

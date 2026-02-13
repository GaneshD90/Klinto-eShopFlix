using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Sagas.DTOs;
using OrderService.Application.Sagas.Services;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// Implementation of saga monitoring service using EF Core.
/// </summary>
public sealed class SagaMonitoringService : ISagaMonitoringService
{
    private readonly OrderServiceDbContext _dbContext;
    private readonly ILogger<SagaMonitoringService> _logger;

    public SagaMonitoringService(
        OrderServiceDbContext dbContext,
        ILogger<SagaMonitoringService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<SagaStateSummaryDto>> GetSagaSummariesAsync(
        string? sagaType = null,
        string? state = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        var results = new List<SagaStateSummaryDto>();

        // Query Checkout Sagas
        if (string.IsNullOrEmpty(sagaType) || sagaType.Equals("Checkout", StringComparison.OrdinalIgnoreCase))
        {
            var checkoutQuery = _dbContext.CheckoutSagaStates.AsQueryable();
            if (!string.IsNullOrEmpty(state)) checkoutQuery = checkoutQuery.Where(s => s.CurrentState == state);
            if (fromDate.HasValue) checkoutQuery = checkoutQuery.Where(s => s.StartedAt >= fromDate);
            if (toDate.HasValue) checkoutQuery = checkoutQuery.Where(s => s.StartedAt <= toDate);

            var checkoutSagas = await checkoutQuery
                .OrderByDescending(s => s.StartedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SagaStateSummaryDto(
                    s.CorrelationId,
                    "Checkout",
                    s.CurrentState,
                    s.CorrelationId, // OrderId is same as CorrelationId
                    s.OrderNumber,
                    s.StartedAt,
                    s.CompletedAt,
                    s.FailureReason))
                .ToListAsync(ct);

            results.AddRange(checkoutSagas);
        }

        // Query Cancellation Sagas
        if (string.IsNullOrEmpty(sagaType) || sagaType.Equals("Cancellation", StringComparison.OrdinalIgnoreCase))
        {
            var cancellationQuery = _dbContext.CancellationSagaStates.AsQueryable();
            if (!string.IsNullOrEmpty(state)) cancellationQuery = cancellationQuery.Where(s => s.CurrentState == state);
            if (fromDate.HasValue) cancellationQuery = cancellationQuery.Where(s => s.RequestedAt >= fromDate);
            if (toDate.HasValue) cancellationQuery = cancellationQuery.Where(s => s.RequestedAt <= toDate);

            var cancellationSagas = await cancellationQuery
                .OrderByDescending(s => s.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SagaStateSummaryDto(
                    s.CorrelationId,
                    "Cancellation",
                    s.CurrentState,
                    s.OrderId,
                    s.OrderNumber,
                    s.RequestedAt,
                    s.CompletedAt,
                    s.FailureReason))
                .ToListAsync(ct);

            results.AddRange(cancellationSagas);
        }

        // Query Return/Refund Sagas
        if (string.IsNullOrEmpty(sagaType) || sagaType.Equals("ReturnRefund", StringComparison.OrdinalIgnoreCase))
        {
            var returnQuery = _dbContext.ReturnRefundSagaStates.AsQueryable();
            if (!string.IsNullOrEmpty(state)) returnQuery = returnQuery.Where(s => s.CurrentState == state);
            if (fromDate.HasValue) returnQuery = returnQuery.Where(s => s.RequestedAt >= fromDate);
            if (toDate.HasValue) returnQuery = returnQuery.Where(s => s.RequestedAt <= toDate);

            var returnSagas = await returnQuery
                .OrderByDescending(s => s.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SagaStateSummaryDto(
                    s.CorrelationId,
                    "ReturnRefund",
                    s.CurrentState,
                    s.OrderId,
                    s.OrderNumber,
                    s.RequestedAt,
                    s.CompletedAt,
                    s.FailureReason))
                .ToListAsync(ct);

            results.AddRange(returnSagas);
        }

        return results.OrderByDescending(s => s.StartedAt);
    }

    public async Task<CheckoutSagaDetailsDto?> GetCheckoutSagaAsync(Guid correlationId, CancellationToken ct = default)
    {
        var saga = await _dbContext.CheckoutSagaStates
            .FirstOrDefaultAsync(s => s.CorrelationId == correlationId, ct);

        if (saga == null) return null;

        return new CheckoutSagaDetailsDto(
            saga.CorrelationId,
            saga.CurrentState,
            saga.CorrelationId, // OrderId is same as CorrelationId
            saga.OrderNumber,
            saga.CustomerId,
            saga.CustomerEmail,
            saga.CartId ?? Guid.Empty,
            saga.TotalAmount,
            saga.CurrencyCode,
            saga.ItemCount,
            saga.StartedAt,
            saga.InventoryReservedAt,
            saga.PaymentAuthorizedAt,
            saga.ConfirmedAt,
            null, // CartDeactivatedAt - not in current state
            saga.CompletedAt,
            saga.FailureReason,
            saga.FailedStep);
    }

    public async Task<CancellationSagaDetailsDto?> GetCancellationSagaAsync(Guid correlationId, CancellationToken ct = default)
    {
        var saga = await _dbContext.CancellationSagaStates
            .FirstOrDefaultAsync(s => s.CorrelationId == correlationId, ct);

        if (saga == null) return null;

        return new CancellationSagaDetailsDto(
            saga.CorrelationId,
            saga.CurrentState,
            saga.OrderId,
            saga.OrderNumber,
            saga.CustomerId,
            saga.CustomerEmail,
            saga.OrderAmount,
            saga.CancellationType,
            saga.CancellationReason,
            saga.RequestedAt,
            saga.StockReleasedAt,
            saga.RefundInitiatedAt,
            saga.CompletedAt,
            saga.RefundAmount,
            saga.RefundTransactionId,
            saga.FailureReason,
            saga.FailedStep);
    }

    public async Task<ReturnRefundSagaDetailsDto?> GetReturnRefundSagaAsync(Guid correlationId, CancellationToken ct = default)
    {
        var saga = await _dbContext.ReturnRefundSagaStates
            .FirstOrDefaultAsync(s => s.CorrelationId == correlationId, ct);

        if (saga == null) return null;

        return new ReturnRefundSagaDetailsDto(
            saga.CorrelationId,
            saga.CurrentState,
            saga.OrderId,
            saga.OrderNumber,
            saga.CustomerId,
            saga.CustomerEmail,
            saga.ReturnNumber,
            saga.ReturnType,
            saga.ReturnReason,
            saga.TotalItemsToReturn,
            saga.RefundAmount,
            saga.RequestedAt,
            saga.ValidatedAt,
            saga.RestockedAt,
            saga.RefundProcessedAt,
            saga.CompletedAt,
            saga.RefundMethod,
            saga.FailureReason,
            saga.FailedStep);
    }

    public async Task<IEnumerable<SagaStateSummaryDto>> GetSagasByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var results = new List<SagaStateSummaryDto>();

        // For Checkout saga, CorrelationId = OrderId
        var checkoutSagas = await _dbContext.CheckoutSagaStates
            .Where(s => s.CorrelationId == orderId)
            .Select(s => new SagaStateSummaryDto(
                s.CorrelationId, "Checkout", s.CurrentState, s.CorrelationId, s.OrderNumber,
                s.StartedAt, s.CompletedAt, s.FailureReason))
            .ToListAsync(ct);
        results.AddRange(checkoutSagas);

        var cancellationSagas = await _dbContext.CancellationSagaStates
            .Where(s => s.OrderId == orderId)
            .Select(s => new SagaStateSummaryDto(
                s.CorrelationId, "Cancellation", s.CurrentState, s.OrderId, s.OrderNumber,
                s.RequestedAt, s.CompletedAt, s.FailureReason))
            .ToListAsync(ct);
        results.AddRange(cancellationSagas);

        var returnSagas = await _dbContext.ReturnRefundSagaStates
            .Where(s => s.OrderId == orderId)
            .Select(s => new SagaStateSummaryDto(
                s.CorrelationId, "ReturnRefund", s.CurrentState, s.OrderId, s.OrderNumber,
                s.RequestedAt, s.CompletedAt, s.FailureReason))
            .ToListAsync(ct);
        results.AddRange(returnSagas);

        return results.OrderByDescending(s => s.StartedAt);
    }

    public async Task<IEnumerable<SagaStatisticsDto>> GetSagaStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var stats = new List<SagaStatisticsDto>();

        // Checkout saga stats
        var checkoutQuery = _dbContext.CheckoutSagaStates.AsQueryable();
        if (fromDate.HasValue) checkoutQuery = checkoutQuery.Where(s => s.StartedAt >= fromDate);
        if (toDate.HasValue) checkoutQuery = checkoutQuery.Where(s => s.StartedAt <= toDate);

        var checkoutStats = await checkoutQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Completed = g.Count(s => s.CurrentState == "Completed"),
                Failed = g.Count(s => s.CurrentState == "Failed"),
                InProgress = g.Count(s => s.CurrentState != "Completed" && s.CurrentState != "Failed")
            })
            .FirstOrDefaultAsync(ct);

        if (checkoutStats != null && checkoutStats.Total > 0)
        {
            stats.Add(new SagaStatisticsDto(
                "Checkout",
                checkoutStats.Total,
                checkoutStats.Completed,
                checkoutStats.Failed,
                checkoutStats.InProgress,
                0, // Would need actual duration calculation
                checkoutStats.Total > 0 ? (double)checkoutStats.Completed / checkoutStats.Total * 100 : 0));
        }

        // Similar for other saga types...
        return stats;
    }

    public async Task<Dictionary<string, int>> GetSagaStateCountsAsync(string sagaType, CancellationToken ct = default)
    {
        var counts = new Dictionary<string, int>();

        switch (sagaType.ToLowerInvariant())
        {
            case "checkout":
                var checkoutCounts = await _dbContext.CheckoutSagaStates
                    .GroupBy(s => s.CurrentState)
                    .Select(g => new { State = g.Key, Count = g.Count() })
                    .ToListAsync(ct);
                foreach (var c in checkoutCounts) counts[c.State] = c.Count;
                break;

            case "cancellation":
                var cancellationCounts = await _dbContext.CancellationSagaStates
                    .GroupBy(s => s.CurrentState)
                    .Select(g => new { State = g.Key, Count = g.Count() })
                    .ToListAsync(ct);
                foreach (var c in cancellationCounts) counts[c.State] = c.Count;
                break;

            case "returnrefund":
                var returnCounts = await _dbContext.ReturnRefundSagaStates
                    .GroupBy(s => s.CurrentState)
                    .Select(g => new { State = g.Key, Count = g.Count() })
                    .ToListAsync(ct);
                foreach (var c in returnCounts) counts[c.State] = c.Count;
                break;
        }

        return counts;
    }
}

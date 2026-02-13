using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Sagas.Services;

namespace OrderService.API.Controllers;

/// <summary>
/// API controller for monitoring saga states.
/// Provides endpoints for admin dashboard and operational monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SagaMonitoringController : ControllerBase
{
    private readonly ISagaMonitoringService _sagaMonitoringService;
    private readonly ILogger<SagaMonitoringController> _logger;

    public SagaMonitoringController(
        ISagaMonitoringService sagaMonitoringService,
        ILogger<SagaMonitoringController> logger)
    {
        _sagaMonitoringService = sagaMonitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of saga summaries.
    /// </summary>
    /// <param name="sagaType">Filter by saga type (Checkout, Cancellation, ReturnRefund)</param>
    /// <param name="state">Filter by current state</param>
    /// <param name="fromDate">Filter by start date (from)</param>
    /// <param name="toDate">Filter by start date (to)</param>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 50)</param>
    [HttpGet]
    public async Task<IActionResult> GetSagas(
        [FromQuery] string? sagaType = null,
        [FromQuery] string? state = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var sagas = await _sagaMonitoringService.GetSagaSummariesAsync(
            sagaType, state, fromDate, toDate, page, pageSize, ct);
        return Ok(sagas);
    }

    /// <summary>
    /// Gets checkout saga details by correlation ID.
    /// </summary>
    [HttpGet("checkout/{correlationId:guid}")]
    public async Task<IActionResult> GetCheckoutSaga(Guid correlationId, CancellationToken ct = default)
    {
        var saga = await _sagaMonitoringService.GetCheckoutSagaAsync(correlationId, ct);
        if (saga == null) return NotFound();
        return Ok(saga);
    }

    /// <summary>
    /// Gets cancellation saga details by correlation ID.
    /// </summary>
    [HttpGet("cancellation/{correlationId:guid}")]
    public async Task<IActionResult> GetCancellationSaga(Guid correlationId, CancellationToken ct = default)
    {
        var saga = await _sagaMonitoringService.GetCancellationSagaAsync(correlationId, ct);
        if (saga == null) return NotFound();
        return Ok(saga);
    }

    /// <summary>
    /// Gets return/refund saga details by correlation ID.
    /// </summary>
    [HttpGet("return/{correlationId:guid}")]
    public async Task<IActionResult> GetReturnRefundSaga(Guid correlationId, CancellationToken ct = default)
    {
        var saga = await _sagaMonitoringService.GetReturnRefundSagaAsync(correlationId, ct);
        if (saga == null) return NotFound();
        return Ok(saga);
    }

    /// <summary>
    /// Gets all sagas associated with a specific order.
    /// </summary>
    [HttpGet("by-order/{orderId:guid}")]
    public async Task<IActionResult> GetSagasByOrderId(Guid orderId, CancellationToken ct = default)
    {
        var sagas = await _sagaMonitoringService.GetSagasByOrderIdAsync(orderId, ct);
        return Ok(sagas);
    }

    /// <summary>
    /// Gets saga statistics (counts, success rates, etc.).
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var stats = await _sagaMonitoringService.GetSagaStatisticsAsync(fromDate, toDate, ct);
        return Ok(stats);
    }

    /// <summary>
    /// Gets count of sagas in each state for a specific saga type.
    /// </summary>
    [HttpGet("state-counts/{sagaType}")]
    public async Task<IActionResult> GetStateCountsBySagaType(string sagaType, CancellationToken ct = default)
    {
        var counts = await _sagaMonitoringService.GetSagaStateCountsAsync(sagaType, ct);
        return Ok(counts);
    }
}

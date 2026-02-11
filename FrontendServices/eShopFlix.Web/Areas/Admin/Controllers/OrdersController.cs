using eShopFlix.Web.HttpClients;
using eShopFlix.Web.Models.Order;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Areas.Admin.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly OrderServiceClient _orderClient;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderServiceClient orderClient, ILogger<OrdersController> logger)
        {
            _orderClient = orderClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string? term = null, string? status = null, int page = 1, CancellationToken ct = default)
        {
            var result = await _orderClient.SearchAsync(
                term: term,
                orderStatus: status,
                page: page,
                pageSize: 15,
                ct: ct);

            ViewBag.SearchTerm = term;
            ViewBag.StatusFilter = status;

            return View(result ?? new PagedResultModel<OrderListItemModel>());
        }

        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var order = await _orderClient.GetByIdAsync(id, ct);
            if (order == null)
                return NotFound();

            var paymentsTask = _orderClient.GetPaymentsAsync(id, ct);
            var shipmentsTask = _orderClient.GetShipmentsAsync(id, ct);
            var timelineTask = _orderClient.GetTimelineAsync(id, customerVisibleOnly: false, ct);
            var notesTask = _orderClient.GetNotesAsync(id, ct);

            await Task.WhenAll(paymentsTask, shipmentsTask, timelineTask, notesTask);

            ViewBag.Payments = await paymentsTask;
            ViewBag.Shipments = await shipmentsTask;
            ViewBag.Timeline = await timelineTask;
            ViewBag.Notes = await notesTask;

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid orderId, string newStatus, string? reason, CancellationToken ct)
        {
            try
            {
                if (newStatus == "Confirmed")
                {
                    var result = await _orderClient.ConfirmAsync(orderId, ct);
                    if (result == null)
                    {
                        TempData["Error"] = "Failed to confirm order.";
                        return RedirectToAction("Details", new { id = orderId });
                    }
                }

                TempData["Success"] = $"Order status updated to {newStatus}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status to {Status}", orderId, newStatus);
                TempData["Error"] = "Failed to update order status.";
            }

            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid orderId, string reason, CancellationToken ct)
        {
            try
            {
                var adminGuid = CurrentUser != null ? CreateDeterministicGuid(CurrentUser.UserId) : (Guid?)null;

                var result = await _orderClient.CancelAsync(
                    orderId, "AdminCancellation", reason, adminGuid, "Admin", ct);

                if (result != null)
                    TempData["Success"] = "Order cancelled successfully.";
                else
                    TempData["Error"] = "Failed to cancel order.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                TempData["Error"] = "Failed to cancel order.";
            }

            return RedirectToAction("Details", new { id = orderId });
        }
    }
}

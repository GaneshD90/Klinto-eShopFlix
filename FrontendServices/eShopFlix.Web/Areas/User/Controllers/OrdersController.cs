using eShopFlix.Web.HttpClients;
using eShopFlix.Web.Models.Order;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Areas.User.Controllers
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

        public async Task<IActionResult> Index(string? status = null, int page = 1, CancellationToken ct = default)
        {
            if (CurrentUser == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var customerGuid = CreateDeterministicGuid(CurrentUser.UserId);

            PagedResultModel<OrderListItemModel>? result;
            if (!string.IsNullOrWhiteSpace(status))
            {
                result = await _orderClient.SearchAsync(
                    customerId: customerGuid, orderStatus: status, page: page, pageSize: 10, ct: ct);
            }
            else
            {
                result = await _orderClient.GetByCustomerAsync(customerGuid, page, pageSize: 10, ct);
            }

            ViewBag.StatusFilter = status;
            return View(result ?? new PagedResultModel<OrderListItemModel>());
        }

        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            if (CurrentUser == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var order = await _orderClient.GetByIdAsync(id, ct);
            if (order == null)
                return NotFound();

            var paymentsTask = _orderClient.GetPaymentsAsync(id, ct);
            var shipmentsTask = _orderClient.GetShipmentsAsync(id, ct);
            var timelineTask = _orderClient.GetTimelineAsync(id, customerVisibleOnly: true, ct);

            await Task.WhenAll(paymentsTask, shipmentsTask, timelineTask);

            ViewBag.Payments = await paymentsTask;
            ViewBag.Shipments = await shipmentsTask;
            ViewBag.Timeline = await timelineTask;

            return View(order);
        }

        public async Task<IActionResult> Track(Guid id, CancellationToken ct)
        {
            if (CurrentUser == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var order = await _orderClient.GetByIdAsync(id, ct);
            if (order == null)
                return NotFound();

            var tracking = await _orderClient.GetShipmentTrackingAsync(id, ct);
            var timeline = await _orderClient.GetTimelineAsync(id, customerVisibleOnly: true, ct);

            ViewBag.Order = order;
            ViewBag.Timeline = timeline;

            return View(tracking);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid orderId, string reason, CancellationToken ct)
        {
            if (CurrentUser == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            try
            {
                var customerGuid = CreateDeterministicGuid(CurrentUser.UserId);

                var result = await _orderClient.CancelAsync(
                    orderId, "CustomerCancellation", reason, customerGuid, "Customer", ct);

                if (result != null)
                    TempData["Success"] = "Your order has been cancelled.";
                else
                    TempData["Error"] = "Unable to cancel this order. It may have already been processed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} by user", orderId);
                TempData["Error"] = "Something went wrong while cancelling your order.";
            }

            return RedirectToAction("Details", new { id = orderId });
        }
    }
}

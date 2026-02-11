using eShopFlix.Web.HttpClients;
using eShopFlix.Web.Models.Order;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        private readonly OrderServiceClient _orderClient;

        public HomeController(OrderServiceClient orderClient)
        {
            _orderClient = orderClient;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            IReadOnlyList<OrderListItemModel> recentOrders = [];
            int pendingCount = 0;
            decimal todayRevenue = 0;

            try
            {
                var searchTask = _orderClient.SearchAsync(page: 1, pageSize: 5, ct: ct);
                var pendingTask = _orderClient.SearchAsync(orderStatus: "Pending", page: 1, pageSize: 1, ct: ct);

                await Task.WhenAll(searchTask, pendingTask);

                var result = await searchTask;
                recentOrders = result?.Items ?? [];

                var pending = await pendingTask;
                pendingCount = pending?.TotalCount ?? 0;

                todayRevenue = recentOrders
                    .Where(o => o.OrderDate.Date == DateTime.UtcNow.Date)
                    .Sum(o => o.TotalAmount);
            }
            catch
            {
                // graceful degradation
            }

            ViewBag.RecentOrders = recentOrders;
            ViewBag.TotalOrders = recentOrders is [] ? 0 : recentOrders.Count;
            ViewBag.PendingCount = pendingCount;
            ViewBag.TodayRevenue = todayRevenue;

            return View();
        }
    }
}

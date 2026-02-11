using eShopFlix.Web.HttpClients;
using eShopFlix.Web.Models.Order;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Areas.User.Controllers
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
            int totalOrders = 0;
            int activeOrders = 0;
            decimal totalSpent = 0;

            if (CurrentUser != null)
            {
                try
                {
                    var customerGuid = CreateDeterministicGuid(CurrentUser.UserId);

                    var recentTask = _orderClient.GetByCustomerAsync(customerGuid, page: 1, pageSize: 5, ct);
                    var allTask = _orderClient.GetByCustomerAsync(customerGuid, page: 1, pageSize: 1, ct);

                    await Task.WhenAll(recentTask, allTask);

                    var recent = await recentTask;
                    recentOrders = recent?.Items ?? [];
                    totalOrders = recent?.TotalCount ?? 0;

                    var all = await allTask;
                    totalOrders = all?.TotalCount ?? totalOrders;

                    // Compute aggregates from recent orders (best-effort)
                    if (recentOrders.Count > 0)
                    {
                        totalSpent = recentOrders.Sum(o => o.TotalAmount);
                        activeOrders = recentOrders.Count(o =>
                            o.OrderStatus is not "Cancelled" and not "Completed" and not "Delivered");
                    }
                }
                catch
                {
                    // graceful degradation
                }
            }

            ViewBag.RecentOrders = recentOrders;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.ActiveOrders = activeOrders;
            ViewBag.TotalSpent = totalSpent;

            return View();
        }
    }
}

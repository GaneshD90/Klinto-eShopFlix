using eShopFlix.Web.HttpClients;
using eShopFlix.Web.Models.Stock;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Areas.Admin.Controllers
{
    public class StockController : BaseController
    {
        private readonly StockServiceClient _stockClient;
        private readonly ILogger<StockController> _logger;

        public StockController(StockServiceClient stockClient, ILogger<StockController> logger)
        {
            _stockClient = stockClient;
            _logger = logger;
        }

        /// <summary>Stock dashboard: warehouses overview, alert count, low stock count.</summary>
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            IReadOnlyList<WarehouseModel> warehouses = [];
            IReadOnlyList<StockAlertModel> alerts = [];
            IReadOnlyList<LowStockReportModel> lowStock = [];

            try
            {
                var whTask = _stockClient.GetWarehousesAsync(ct);
                var alertTask = _stockClient.GetActiveAlertsAsync(ct);
                var lowTask = _stockClient.GetLowStockReportAsync(ct);

                await Task.WhenAll(whTask, alertTask, lowTask);

                warehouses = await whTask;
                alerts = await alertTask;
                lowStock = await lowTask;
            }
            catch
            {
                // graceful degradation
            }

            ViewBag.Warehouses = warehouses;
            ViewBag.Alerts = alerts;
            ViewBag.LowStock = lowStock;

            return View();
        }

        /// <summary>Warehouse list.</summary>
        public async Task<IActionResult> Warehouses(CancellationToken ct)
        {
            var warehouses = await _stockClient.GetWarehousesAsync(ct);
            return View(warehouses);
        }

        /// <summary>Stock items in a specific warehouse.</summary>
        public async Task<IActionResult> WarehouseStock(Guid id, CancellationToken ct)
        {
            var warehouseTask = _stockClient.GetWarehouseAsync(id, ct);
            var stockTask = _stockClient.GetStockByWarehouseAsync(id, ct);

            await Task.WhenAll(warehouseTask, stockTask);

            var warehouse = await warehouseTask;
            if (warehouse == null)
                return NotFound();

            ViewBag.Warehouse = warehouse;
            return View(await stockTask);
        }

        /// <summary>Active alerts.</summary>
        public async Task<IActionResult> Alerts(CancellationToken ct)
        {
            var alerts = await _stockClient.GetActiveAlertsAsync(ct);
            return View(alerts);
        }

        /// <summary>Low stock report.</summary>
        public async Task<IActionResult> LowStock(CancellationToken ct)
        {
            var report = await _stockClient.GetLowStockReportAsync(ct);
            return View(report);
        }

        /// <summary>Increase stock for a stock item.</summary>
        [HttpPost]
        public async Task<IActionResult> IncreaseStock(Guid stockItemId, int quantity, string reason, Guid? warehouseId, CancellationToken ct)
        {
            try
            {
                var adminGuid = CurrentUser != null ? CreateDeterministicGuid(CurrentUser.UserId) : Guid.Empty;
                var result = await _stockClient.IncreaseStockAsync(stockItemId, quantity, reason, adminGuid, ct);

                if (result != null)
                    TempData["Success"] = $"Stock increased by {quantity}. New quantity: {result.QuantityAfter}";
                else
                    TempData["Error"] = "Failed to increase stock.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error increasing stock {StockItemId}", stockItemId);
                TempData["Error"] = "Failed to increase stock.";
            }

            if (warehouseId.HasValue)
                return RedirectToAction("WarehouseStock", new { id = warehouseId.Value });
            return RedirectToAction("Index");
        }

        /// <summary>Decrease stock for a stock item.</summary>
        [HttpPost]
        public async Task<IActionResult> DecreaseStock(Guid stockItemId, int quantity, string reason, Guid? warehouseId, CancellationToken ct)
        {
            try
            {
                var adminGuid = CurrentUser != null ? CreateDeterministicGuid(CurrentUser.UserId) : Guid.Empty;
                var result = await _stockClient.DecreaseStockAsync(stockItemId, quantity, reason, adminGuid, ct);

                if (result != null)
                    TempData["Success"] = $"Stock decreased by {quantity}. New quantity: {result.QuantityAfter}";
                else
                    TempData["Error"] = "Failed to decrease stock.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decreasing stock {StockItemId}", stockItemId);
                TempData["Error"] = "Failed to decrease stock.";
            }

            if (warehouseId.HasValue)
                return RedirectToAction("WarehouseStock", new { id = warehouseId.Value });
            return RedirectToAction("Index");
        }
    }
}

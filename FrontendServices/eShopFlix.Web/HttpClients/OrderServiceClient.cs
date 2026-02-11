using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using eShopFlix.Web.Models.Order;
using Microsoft.Extensions.Logging;

namespace eShopFlix.Web.HttpClients;

/// <summary>
/// HTTP client for communicating with the OrderService via API Gateway.
/// Gateway route: /order/{everything} ? OrderService /api/Order/{everything}
/// </summary>
public class OrderServiceClient
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;
    private readonly ILogger<OrderServiceClient> _logger;

    public OrderServiceClient(HttpClient client, ILogger<OrderServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    // ============ Header Helpers ============

    private static void AddIdempotencyHeaders(HttpRequestMessage req, string deterministicKey)
    {
        req.Headers.TryAddWithoutValidation("x-correlation-id", Guid.NewGuid().ToString("N"));
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(deterministicKey)));
        req.Headers.TryAddWithoutValidation("x-idempotency-key", key);
    }

    private static void AddUniqueIdempotencyHeaders(HttpRequestMessage req, string payload)
    {
        req.Headers.TryAddWithoutValidation("x-correlation-id", Guid.NewGuid().ToString("N"));
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{payload}:{timestamp}")));
        req.Headers.TryAddWithoutValidation("x-idempotency-key", key);
    }

    // ============ Order Queries ============

    /// <summary>Search orders with optional filters.</summary>
    public async Task<PagedResultModel<OrderListItemModel>?> SearchAsync(
        string? term = null, Guid? customerId = null, string? orderStatus = null,
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(term)) queryParams.Add($"term={Uri.EscapeDataString(term)}");
            if (customerId.HasValue) queryParams.Add($"customerId={customerId.Value}");
            if (!string.IsNullOrWhiteSpace(orderStatus)) queryParams.Add($"orderStatus={Uri.EscapeDataString(orderStatus)}");

            var url = $"order/Search?{string.Join("&", queryParams)}";
            return await _client.GetFromJsonAsync<PagedResultModel<OrderListItemModel>>(url, JsonOpts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching orders");
            return null;
        }
    }

    /// <summary>Get order by ID with full details.</summary>
    public async Task<OrderDetailModel?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<OrderDetailModel>($"order/GetById/{orderId}", JsonOpts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", orderId);
            return null;
        }
    }

    /// <summary>Get orders for a customer (paged).</summary>
    public async Task<PagedResultModel<OrderListItemModel>?> GetByCustomerAsync(
        Guid customerId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<PagedResultModel<OrderListItemModel>>(
                $"order/GetByCustomer/{customerId}?page={page}&pageSize={pageSize}", JsonOpts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
            return null;
        }
    }

    // ============ Order Lifecycle Commands ============

    /// <summary>Create an order from cart (checkout flow).</summary>
    public async Task<CreateOrderFromCartResultModel?> CreateFromCartAsync(
        Guid cartId, Guid customerId, string customerEmail, string orderSource,
        string? billingAddressJson = null, string? shippingAddressJson = null,
        string? paymentMethod = null, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                CartId = cartId,
                CustomerId = customerId,
                CustomerEmail = customerEmail,
                OrderSource = orderSource,
                BillingAddressJson = billingAddressJson,
                ShippingAddressJson = shippingAddressJson,
                PaymentMethod = paymentMethod
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "order/CreateFromCart")
            {
                Content = JsonContent.Create(payload)
            };
            AddUniqueIdempotencyHeaders(req, $"createfromcart:{cartId}:{customerId}");

            using var resp = await _client.SendAsync(req, ct);
            var content = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("CreateFromCart failed: {Status} - {Content}", resp.StatusCode, content);
                return null;
            }

            return JsonSerializer.Deserialize<CreateOrderFromCartResultModel>(content, JsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order from cart {CartId}", cartId);
            return null;
        }
    }

    /// <summary>Confirm an order.</summary>
    public async Task<ConfirmOrderResultModel?> ConfirmAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            using var resp = await _client.PostAsync($"order/Confirm/{orderId}", null, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ConfirmOrderResultModel>(JsonOpts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming order {OrderId}", orderId);
            return null;
        }
    }

    /// <summary>Cancel an order.</summary>
    public async Task<OrderDetailModel?> CancelAsync(
        Guid orderId, string cancellationType, string cancellationReason,
        Guid? cancelledBy = null, string? cancelledByType = null, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                CancellationType = cancellationType,
                CancellationReason = cancellationReason,
                CancelledBy = cancelledBy,
                CancelledByType = cancelledByType
            };

            using var resp = await _client.PostAsJsonAsync($"order/Cancel/{orderId}", payload, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<OrderDetailModel>(JsonOpts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            return null;
        }
    }

    // ============ Status & Timeline ============

    /// <summary>Get status history for an order.</summary>
    public async Task<IReadOnlyList<OrderStatusHistoryModel>> GetStatusHistoryAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<OrderStatusHistoryModel>>(
                $"order/GetStatusHistory/{orderId}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status history for order {OrderId}", orderId);
            return [];
        }
    }

    /// <summary>Get timeline events for an order.</summary>
    public async Task<IReadOnlyList<OrderTimelineModel>> GetTimelineAsync(
        Guid orderId, bool customerVisibleOnly = false, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<OrderTimelineModel>>(
                $"order/GetTimeline/{orderId}?customerVisibleOnly={customerVisibleOnly}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timeline for order {OrderId}", orderId);
            return [];
        }
    }

    // ============ Payments ============

    /// <summary>Process a payment for an order.</summary>
    public async Task<ProcessPaymentResultModel?> ProcessPaymentAsync(
        Guid orderId, string paymentMethod, string paymentProvider, decimal amount,
        string? transactionId = null, string? authorizationCode = null,
        string? gatewayResponse = null, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                PaymentMethod = paymentMethod,
                PaymentProvider = paymentProvider,
                Amount = amount,
                TransactionId = transactionId,
                AuthorizationCode = authorizationCode,
                PaymentGatewayResponse = gatewayResponse
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, $"order/ProcessPayment/{orderId}")
            {
                Content = JsonContent.Create(payload)
            };
            AddUniqueIdempotencyHeaders(req, $"payment:{orderId}:{amount}");

            using var resp = await _client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ProcessPaymentResultModel>(JsonOpts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", orderId);
            return null;
        }
    }

    /// <summary>Get all payments for an order.</summary>
    public async Task<IReadOnlyList<OrderPaymentModel>> GetPaymentsAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<OrderPaymentModel>>(
                $"order/GetPayments/{orderId}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for order {OrderId}", orderId);
            return [];
        }
    }

    // ============ Shipments ============

    /// <summary>Get all shipments for an order.</summary>
    public async Task<IReadOnlyList<OrderShipmentModel>> GetShipmentsAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<OrderShipmentModel>>(
                $"order/GetShipments/{orderId}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipments for order {OrderId}", orderId);
            return [];
        }
    }

    /// <summary>Get shipment tracking for an order.</summary>
    public async Task<IReadOnlyList<ShipmentTrackingModel>> GetShipmentTrackingAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<ShipmentTrackingModel>>(
                $"order/GetShipmentTracking/{orderId}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipment tracking for order {OrderId}", orderId);
            return [];
        }
    }

    // ============ Returns & Refunds ============

    /// <summary>Create a return request.</summary>
    public async Task<CommandResultModel?> CreateReturnRequestAsync(
        Guid orderId, Guid customerId, string returnType, string returnReason,
        string returnItemsJson, string? customerComments = null, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                CustomerId = customerId,
                ReturnType = returnType,
                ReturnReason = returnReason,
                ReturnItemsJson = returnItemsJson,
                CustomerComments = customerComments
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, $"order/CreateReturnRequest/{orderId}")
            {
                Content = JsonContent.Create(payload)
            };
            AddUniqueIdempotencyHeaders(req, $"return:{orderId}:{customerId}");

            using var resp = await _client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<CommandResultModel>(JsonOpts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating return request for order {OrderId}", orderId);
            return null;
        }
    }

    /// <summary>Get returns for an order.</summary>
    public async Task<IReadOnlyList<OrderReturnModel>> GetReturnsAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<OrderReturnModel>>(
                $"order/GetReturns/{orderId}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting returns for order {OrderId}", orderId);
            return [];
        }
    }

    /// <summary>Get refunds for an order.</summary>
    public async Task<IReadOnlyList<OrderRefundModel>> GetRefundsAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<OrderRefundModel>>(
                $"order/GetRefunds/{orderId}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refunds for order {OrderId}", orderId);
            return [];
        }
    }

    // ============ Notes ============

    /// <summary>Get notes for an order.</summary>
    public async Task<IReadOnlyList<OrderNoteModel>> GetNotesAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<OrderNoteModel>>(
                $"order/GetNotes/{orderId}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notes for order {OrderId}", orderId);
            return [];
        }
    }

    // ============ Items ============

    /// <summary>Get items for an order.</summary>
    public async Task<IReadOnlyList<OrderItemModel>> GetOrderItemsAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<OrderItemModel>>(
                $"order/GetOrderItems/{orderId}", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items for order {OrderId}", orderId);
            return [];
        }
    }

    // ============ Customer Analytics ============

    /// <summary>Get customer order history analytics.</summary>
    public async Task<IReadOnlyList<CustomerOrderHistoryModel>> GetCustomerOrderHistoryAsync(CancellationToken ct = default)
    {
        try
        {
            return await _client.GetFromJsonAsync<IReadOnlyList<CustomerOrderHistoryModel>>(
                "order/GetCustomerOrderHistory", JsonOpts, ct) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer order history");
            return [];
        }
    }
}

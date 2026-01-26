using System.Net;
using System.Text;
using System.Text.Json;

namespace Contracts.Resilience;

/// <summary>
/// Context-aware fallback responses for resilience scenarios.
/// Use these when downstream services are unavailable to provide meaningful responses.
/// </summary>
public static class FallbackResponses
{
    /// <summary>
    /// Gets a fallback response based on the operation key
    /// </summary>
    public static FallbackResult GetFallback(string operationKey)
    {
        return operationKey.ToLowerInvariant() switch
        {
            // Cart operations - be optimistic, allow user to continue
            "getcart" or "cart" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    id = 0,
                    userId = 0,
                    cartItems = Array.Empty<object>(),
                    total = 0m,
                    tax = 0m,
                    grandTotal = 0m,
                    _fallback = true,
                    message = "Cart service temporarily unavailable. Please refresh."
                }),

            "getcartcount" or "cartcount" => new FallbackResult(HttpStatusCode.OK, 0),

            "addtocart" => new FallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new
                {
                    success = false,
                    error = "Unable to add to cart. Please try again.",
                    retryAfterSeconds = 5
                }),

            // Stock operations - be optimistic for better UX
            "checkstock" or "checkavailability" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    isAvailable = true, // Optimistic - validate at checkout
                    availableQuantity = 999,
                    _fallback = true,
                    message = "Stock check pending. Availability confirmed at checkout."
                }),

            "reservestock" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    success = true,
                    reservationId = (string?)null,
                    _fallback = true,
                    message = "Reservation pending confirmation."
                }),

            "getreservations" or "cartreservations" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    reservations = Array.Empty<object>(),
                    _fallback = true
                }),

            // Payment operations - be strict, don't proceed without confirmation
            "createorder" or "createpaymentorder" => new FallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new
                {
                    success = false,
                    error = "Payment service temporarily unavailable. Please try again in a moment.",
                    retryAfterSeconds = 30
                }),

            "verifypayment" => new FallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new
                {
                    success = false,
                    status = "pending",
                    message = "Payment verification in progress. You'll receive confirmation shortly."
                }),

            // Order operations
            "getorders" or "getuserorders" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    orders = Array.Empty<object>(),
                    totalCount = 0,
                    _fallback = true,
                    message = "Order history temporarily unavailable."
                }),

            "getorder" or "getorderbyid" => new FallbackResult(
                HttpStatusCode.NotFound,
                new
                {
                    error = "Order details temporarily unavailable.",
                    retryAfterSeconds = 10
                }),

            "placeorder" => new FallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new
                {
                    success = false,
                    error = "Order service unavailable. Your cart is saved.",
                    retryAfterSeconds = 30
                }),

            // Catalog operations - return empty but valid data
            "getproducts" or "products" or "catalog" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    products = Array.Empty<object>(),
                    totalCount = 0,
                    page = 1,
                    pageSize = 10,
                    _fallback = true,
                    message = "Product catalog loading. Please refresh."
                }),

            "getproductbyid" or "product" => new FallbackResult(
                HttpStatusCode.NotFound,
                new
                {
                    error = "Product information temporarily unavailable.",
                    _fallback = true
                }),

            "getcategories" or "categories" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    categories = Array.Empty<object>(),
                    _fallback = true
                }),

            // Auth operations - fail-safe (deny when uncertain)
            "validatetoken" or "authenticate" => new FallbackResult(
                HttpStatusCode.Unauthorized,
                new
                {
                    isValid = false,
                    message = "Authentication service unavailable. Please login again."
                }),

            "getuserinfo" or "userinfo" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    userId = 0,
                    isAuthenticated = false,
                    _fallback = true,
                    message = "User service unavailable."
                }),

            // Totals/Summary operations
            "gettotals" or "carttotals" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    subtotal = 0m,
                    tax = 0m,
                    shipping = 0m,
                    discount = 0m,
                    total = 0m,
                    _fallback = true
                }),

            "getcoupons" or "coupons" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    coupons = Array.Empty<object>(),
                    _fallback = true
                }),

            "getshipments" or "shipping" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    shipments = Array.Empty<object>(),
                    _fallback = true
                }),

            "getsavedforlater" or "savedforlater" => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    items = Array.Empty<object>(),
                    _fallback = true
                }),

            // Default - safe empty response
            _ => new FallbackResult(
                HttpStatusCode.OK,
                new
                {
                    data = (object?)null,
                    success = false,
                    _fallback = true,
                    message = "Service temporarily unavailable.",
                    timestamp = DateTime.UtcNow
                })
        };
    }

    /// <summary>
    /// Creates an HttpResponseMessage from a FallbackResult
    /// </summary>
    public static HttpResponseMessage ToHttpResponse(this FallbackResult fallback)
    {
        var json = JsonSerializer.Serialize(fallback.Content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return new HttpResponseMessage(fallback.StatusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            Headers =
            {
                { "X-Fallback-Response", "true" },
                { "X-Fallback-Timestamp", DateTime.UtcNow.ToString("O") }
            }
        };
    }
}

/// <summary>
/// Represents a fallback response with status code and content
/// </summary>
public record FallbackResult(HttpStatusCode StatusCode, object Content);

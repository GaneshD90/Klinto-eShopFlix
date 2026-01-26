using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Contracts.Resilience;

/// <summary>
/// Mobile/Client-friendly error responses following RFC 7807 Problem Details.
/// Use these at the API Gateway or Controller level for external clients (Mobile, Web, etc.).
/// </summary>
public static class ClientFallbackResponses
{
    /// <summary>
    /// Gets a client-friendly fallback response based on the service/operation.
    /// Returns RFC 7807 Problem Details format that mobile apps can easily parse.
    /// </summary>
    public static ClientFallbackResult GetClientFallback(string serviceName, string? operationName = null)
    {
        var key = string.IsNullOrEmpty(operationName) 
            ? serviceName.ToLowerInvariant() 
            : $"{serviceName}:{operationName}".ToLowerInvariant();

        return key switch
        {
            // Catalog Service
            "catalog" or "catalog:getproducts" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:catalog-unavailable",
                    Title = "Product catalog temporarily unavailable",
                    Status = 503,
                    Detail = "We're having trouble loading products. Please pull to refresh or try again in a moment.",
                    Instance = $"/catalog",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 5,
                        ["canRetry"] = true,
                        ["showOfflineData"] = true, // Hint: Mobile app can show cached data
                        ["errorCode"] = "CATALOG_UNAVAILABLE"
                    }
                }),

            "catalog:getproduct" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:product-unavailable",
                    Title = "Product details unavailable",
                    Status = 503,
                    Detail = "Unable to load product details. Please try again.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 3,
                        ["canRetry"] = true,
                        ["errorCode"] = "PRODUCT_UNAVAILABLE"
                    }
                }),

            // Cart Service
            "cart" or "cart:getcart" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:cart-unavailable",
                    Title = "Cart temporarily unavailable",
                    Status = 503,
                    Detail = "We're having trouble loading your cart. Your items are safe - please try again.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 3,
                        ["canRetry"] = true,
                        ["showOfflineCart"] = true, // Hint: Show locally cached cart
                        ["errorCode"] = "CART_UNAVAILABLE"
                    }
                }),

            "cart:addtocart" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:cart-add-failed",
                    Title = "Unable to add to cart",
                    Status = 503,
                    Detail = "We couldn't add this item to your cart. Please try again.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 2,
                        ["canRetry"] = true,
                        ["queueOffline"] = true, // Hint: Queue action for when online
                        ["errorCode"] = "CART_ADD_FAILED"
                    }
                }),

            "cart:checkout" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:checkout-unavailable",
                    Title = "Checkout temporarily unavailable",
                    Status = 503,
                    Detail = "We're experiencing issues with checkout. Your cart is saved. Please try again in a moment.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 10,
                        ["canRetry"] = true,
                        ["cartSaved"] = true,
                        ["errorCode"] = "CHECKOUT_UNAVAILABLE"
                    }
                }),

            // Stock Service
            "stock" or "stock:check" => new ClientFallbackResult(
                HttpStatusCode.OK, // Return OK with optimistic response
                new ProblemDetails
                {
                    Type = "urn:eshopflix:info:stock-pending",
                    Title = "Stock check pending",
                    Status = 200,
                    Detail = "Stock availability will be confirmed at checkout.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["assumeAvailable"] = true,
                        ["verifyAtCheckout"] = true,
                        ["errorCode"] = "STOCK_PENDING"
                    }
                }),

            // Payment Service
            "payment" or "payment:create" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:payment-unavailable",
                    Title = "Payment service unavailable",
                    Status = 503,
                    Detail = "We're unable to process payments right now. Please try again in a few minutes.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 30,
                        ["canRetry"] = true,
                        ["doNotRetryImmediately"] = true, // Prevent rapid retries
                        ["errorCode"] = "PAYMENT_UNAVAILABLE"
                    }
                }),

            "payment:verify" => new ClientFallbackResult(
                HttpStatusCode.Accepted, // 202 - Processing
                new ProblemDetails
                {
                    Type = "urn:eshopflix:info:payment-pending",
                    Title = "Payment verification in progress",
                    Status = 202,
                    Detail = "Your payment is being processed. You'll receive a confirmation notification shortly.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["checkStatusAfterSeconds"] = 15,
                        ["notificationPending"] = true,
                        ["errorCode"] = "PAYMENT_PENDING"
                    }
                }),

            // Order Service
            "order" or "order:getorders" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:orders-unavailable",
                    Title = "Order history unavailable",
                    Status = 503,
                    Detail = "We're having trouble loading your orders. Please try again.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 5,
                        ["canRetry"] = true,
                        ["showCachedOrders"] = true,
                        ["errorCode"] = "ORDERS_UNAVAILABLE"
                    }
                }),

            "order:place" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:order-failed",
                    Title = "Unable to place order",
                    Status = 503,
                    Detail = "We couldn't place your order right now. Your cart and payment details are saved.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 30,
                        ["canRetry"] = true,
                        ["cartSaved"] = true,
                        ["errorCode"] = "ORDER_FAILED"
                    }
                }),

            "order:track" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:tracking-unavailable",
                    Title = "Order tracking unavailable",
                    Status = 503,
                    Detail = "We're unable to fetch tracking information. Please try again later.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 10,
                        ["canRetry"] = true,
                        ["errorCode"] = "TRACKING_UNAVAILABLE"
                    }
                }),

            // Auth Service
            "auth" or "auth:login" => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:auth-unavailable",
                    Title = "Login temporarily unavailable",
                    Status = 503,
                    Detail = "We're having trouble with authentication. Please try again in a moment.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 5,
                        ["canRetry"] = true,
                        ["allowOfflineMode"] = true, // Hint: Allow browsing without auth
                        ["errorCode"] = "AUTH_UNAVAILABLE"
                    }
                }),

            "auth:refresh" => new ClientFallbackResult(
                HttpStatusCode.Unauthorized,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:session-expired",
                    Title = "Session expired",
                    Status = 401,
                    Detail = "Your session has expired. Please login again.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["requiresLogin"] = true,
                        ["errorCode"] = "SESSION_EXPIRED"
                    }
                }),

            // Default fallback
            _ => new ClientFallbackResult(
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Type = "urn:eshopflix:error:service-unavailable",
                    Title = "Service temporarily unavailable",
                    Status = 503,
                    Detail = "We're experiencing technical difficulties. Please try again.",
                    Extensions = new Dictionary<string, object>
                    {
                        ["retryAfterSeconds"] = 10,
                        ["canRetry"] = true,
                        ["errorCode"] = "SERVICE_UNAVAILABLE"
                    }
                })
        };
    }

    /// <summary>
    /// Creates an HttpResponseMessage from a ClientFallbackResult
    /// </summary>
    public static HttpResponseMessage ToHttpResponse(this ClientFallbackResult fallback)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        var json = JsonSerializer.Serialize(fallback.Problem, options);

        var response = new HttpResponseMessage(fallback.StatusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/problem+json")
        };

        // Add standard headers
        response.Headers.Add("X-Fallback-Response", "true");
        response.Headers.Add("X-Error-Code", fallback.Problem.Extensions.GetValueOrDefault("errorCode")?.ToString() ?? "UNKNOWN");
        
        if (fallback.Problem.Extensions.TryGetValue("retryAfterSeconds", out var retryAfter))
        {
            response.Headers.Add("Retry-After", retryAfter.ToString());
        }

        return response;
    }
}

/// <summary>
/// RFC 7807 Problem Details for HTTP APIs
/// </summary>
public class ProblemDetails
{
    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    public string Type { get; set; } = "about:blank";

    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    public string Title { get; set; } = "An error occurred";

    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// A URI reference that identifies the specific occurrence
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// Additional properties for mobile app handling
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> Extensions { get; set; } = new();
}

/// <summary>
/// Represents a client-friendly fallback response
/// </summary>
public record ClientFallbackResult(HttpStatusCode StatusCode, ProblemDetails Problem);

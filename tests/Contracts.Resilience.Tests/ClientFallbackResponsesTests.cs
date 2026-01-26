using System.Net;
using System.Text.Json;
using Contracts.Resilience;
using Xunit;

namespace Contracts.Resilience.Tests;

public class ClientFallbackResponsesTests
{
    [Theory]
    [InlineData("catalog", null, HttpStatusCode.ServiceUnavailable, "catalog-unavailable")]
    [InlineData("catalog", "getproducts", HttpStatusCode.ServiceUnavailable, "catalog-unavailable")]
    [InlineData("catalog", "getproduct", HttpStatusCode.ServiceUnavailable, "product-unavailable")]
    [InlineData("cart", null, HttpStatusCode.ServiceUnavailable, "cart-unavailable")]
    [InlineData("cart", "addtocart", HttpStatusCode.ServiceUnavailable, "cart-add-failed")]
    [InlineData("cart", "checkout", HttpStatusCode.ServiceUnavailable, "checkout-unavailable")]
    [InlineData("stock", null, HttpStatusCode.OK, "stock-pending")]
    [InlineData("payment", "create", HttpStatusCode.ServiceUnavailable, "payment-unavailable")]
    [InlineData("payment", "verify", HttpStatusCode.Accepted, "payment-pending")]
    [InlineData("order", null, HttpStatusCode.ServiceUnavailable, "orders-unavailable")]
    [InlineData("order", "place", HttpStatusCode.ServiceUnavailable, "order-failed")]
    [InlineData("order", "track", HttpStatusCode.ServiceUnavailable, "tracking-unavailable")]
    [InlineData("auth", null, HttpStatusCode.ServiceUnavailable, "auth-unavailable")]
    [InlineData("auth", "login", HttpStatusCode.ServiceUnavailable, "auth-unavailable")]
    [InlineData("auth", "refresh", HttpStatusCode.Unauthorized, "session-expired")]
    public void GetClientFallback_ReturnsExpectedProblem(string service, string? operation, HttpStatusCode expectedStatus, string expectedTypeSuffix)
    {
        var result = ClientFallbackResponses.GetClientFallback(service, operation);
n        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.NotNull(result.Problem);
        Assert.EndsWith(expectedTypeSuffix, result.Problem.Type);
    }

    [Fact]
    public void ToHttpResponse_SerializesProblemAndAddsHeaders()
    {
        var fallback = ClientFallbackResponses.GetClientFallback("catalog", "getproducts");
        var response = fallback.ToHttpResponse();

        Assert.Equal(fallback.StatusCode, response.StatusCode);
        Assert.True(response.Content.Headers.ContentType.MediaType == "application/problem+json");
        Assert.True(response.Headers.Contains("X-Fallback-Response"));
        Assert.True(response.Headers.Contains("X-Error-Code"));
        var content = response.Content.ReadAsStringAsync().Result;
        var doc = JsonSerializer.Deserialize<ProblemDetails>(content);
        Assert.NotNull(doc);
        Assert.Equal(fallback.Problem.Type, doc!.Type);
    }
}

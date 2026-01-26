using Contracts.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Fallback;
using Serilog;
using System.Net;

namespace OcelotApiGateway.Resilience
{
    /// <summary>
    /// DelegatingHandler applying resilience policies for downstream calls.
    /// Logs standardized events: [Timeout], [Polly-Retry], [CircuitBreaker], [Fallback].
    /// Returns RFC 7807 Problem Details for mobile/client apps.
    /// Attach via ocelot json: "DelegatingHandlers": ["DownstreamResilienceHandler"].
    /// </summary>
    public sealed class DownstreamResilienceHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;
        public DownstreamResilienceHandler() => _policy = BuildPolicy();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var (serviceName, operationName) = InferServiceAndOperation(request);
            var op = $"gateway:{serviceName}";
            var context = new Context(op);
            
            // Store service info in context for fallback
            context["ServiceName"] = serviceName;
            context["OperationName"] = operationName;
            
            return _policy.ExecuteAsync((ctx, ct) => base.SendAsync(request, ct), context, cancellationToken);
        }

        private static (string serviceName, string operationName) InferServiceAndOperation(HttpRequestMessage req)
        {
            var path = req.RequestUri?.AbsolutePath ?? "/";
            var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var method = req.Method.Method.ToUpperInvariant();
            
            // expecting /api/{service}/... or /{service}/...
            var svc = segments.Length > 0 ? segments[0] : "downstream";
            if (svc.Equals("api", StringComparison.OrdinalIgnoreCase) && segments.Length > 1)
            {
                svc = segments[1];
            }

            // Infer operation from method and path
            var operation = InferOperation(method, segments);
            
            return (svc, operation);
        }

        private static string InferOperation(string method, string[] segments)
        {
            // Map HTTP method + path pattern to operation name
            var lastSegment = segments.Length > 0 ? segments[^1].ToLowerInvariant() : "";
            
            return (method, lastSegment) switch
            {
                ("GET", "cart") => "getcart",
                ("POST", "cart") or ("POST", "items") => "addtocart",
                ("POST", "checkout") => "checkout",
                ("GET", "catalog") or ("GET", "products") => "getproducts",
                ("GET", _) when int.TryParse(lastSegment, out _) => "getproduct", // GET /product/123
                ("GET", "orders") => "getorders",
                ("POST", "orders") or ("POST", "order") => "place",
                ("GET", "track") or ("GET", "tracking") => "track",
                ("POST", "payment") or ("POST", "pay") => "create",
                ("POST", "verify") => "verify",
                ("POST", "login") or ("POST", "signin") => "login",
                ("POST", "refresh") => "refresh",
                ("GET", "stock") or ("GET", "availability") => "check",
                _ => "unknown"
            };
        }

        private static IAsyncPolicy<HttpResponseMessage> BuildPolicy()
        {
            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(30), TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (ctx, span, task) =>
                {
                    Log.Error("[Timeout] op={OperationKey} after {Seconds}s", ctx.OperationKey, span.TotalSeconds);
                    return Task.CompletedTask;
                });

            var retry = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
                .Or<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(2,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (outcome, delay, attempt, ctx) =>
                        Log.Warning("[Polly-Retry] op={OperationKey} attempt={Attempt} delay={Delay}s status={Status}",
                            ctx.OperationKey, attempt, delay.TotalSeconds, outcome.Result?.StatusCode));

            var breaker = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                    onBreak: (outcome, span, ctx) =>
                        Log.Warning("[CircuitBreaker] op={OperationKey} OPEN for {Break}s status={Status}",
                            ctx.OperationKey, span.TotalSeconds, outcome.Result?.StatusCode),
                    onReset: ctx => Log.Information("[CircuitBreaker] op={OperationKey} RESET", ctx.OperationKey),
                    onHalfOpen: () => Log.Information("[CircuitBreaker] HALF-OPEN"));

            var fallback = Policy<HttpResponseMessage>
                .Handle<TimeoutRejectedException>()
                .Or<BrokenCircuitException>()
                .Or<HttpRequestException>()
                .FallbackAsync((delegateResult, context, ct) =>
                {
                    // Get service and operation from context
                    var serviceName = context.TryGetValue("ServiceName", out var svc) ? svc?.ToString() ?? "unknown" : "unknown";
                    var operationName = context.TryGetValue("OperationName", out var op) ? op?.ToString() : null;
                    
                    Log.Warning("[Fallback] op={OperationKey} service={Service} operation={Operation} returning client-friendly response",
                        context.OperationKey, serviceName, operationName);
                    
                    // Use ClientFallbackResponses for mobile-friendly error
                    var fallbackResult = ClientFallbackResponses.GetClientFallback(serviceName, operationName);
                    return Task.FromResult(fallbackResult.ToHttpResponse());
                },
                (delegateResult, context) =>
                {
                    Log.Information("[Fallback] op={OperationKey} fallback triggered", context.OperationKey);
                    return Task.CompletedTask;
                });

            // Fallback -> Breaker -> Retry -> Timeout
            return Policy.WrapAsync(fallback, Policy.WrapAsync(breaker, Policy.WrapAsync(retry, timeout)));
        }
    }
}

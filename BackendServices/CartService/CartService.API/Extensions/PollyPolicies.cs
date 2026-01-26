using Contracts.Resilience;
using Polly;
using Polly.Timeout;
using Serilog;
using System.Net;

namespace CartService.Api.Extensions
{
    /// <summary>
    /// Central resilience policy for Cart's outbound HTTP calls.
    /// Wraps: Bulkhead -> Fallback -> CircuitBreaker -> Retry -> Timeout.
    /// Uses context-aware fallback responses from shared Contracts.
    /// </summary>
    public static class PollyPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> CreatePolicy(string operationKey)
            => BuildCompositePolicy(operationKey);

        public static IAsyncPolicy<HttpResponseMessage> CreatePolicy()
            => BuildCompositePolicy("http");

        private static IAsyncPolicy<HttpResponseMessage> BuildCompositePolicy(string operationKey)
        {
            // Retry: 3 attempts with exponential backoff + jitter
            var retry = Policy
                .HandleResult<HttpResponseMessage>(r => ShouldRetry(r.StatusCode))
                .Or<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250)),
                    onRetry: (outcome, delay, attempt, ctx) =>
                        Log.Warning("[Polly-Retry] op={OperationKey} attempt={Attempt} delay={Delay}s status={Status}",
                            ctx.OperationKey ?? operationKey,
                            attempt,
                            Math.Round(delay.TotalSeconds, 3),
                            outcome.Result?.StatusCode ?? (object?)outcome.Exception?.GetType().Name)
                );

            // Circuit Breaker: open after 5 consecutive failures for 30s
            var breaker = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .Or<TaskCanceledException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, span) =>
                        Log.Warning("[CircuitBreaker] op={OperationKey} OPEN for {Break}s status={Status}",
                            operationKey, span.TotalSeconds, outcome.Result?.StatusCode ?? (object?)outcome.Exception?.GetType().Name),
                    onReset: () =>
                        Log.Information("[CircuitBreaker] op={OperationKey} RESET", operationKey),
                    onHalfOpen: () =>
                        Log.Information("[CircuitBreaker] op={OperationKey} HALF-OPEN", operationKey)
                );

            // Timeout: 20s cap on outbound calls
            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(20),
                timeoutStrategy: TimeoutStrategy.Optimistic,
                onTimeoutAsync: (context, span, task, ct) =>
                {
                    Log.Error("[Timeout] op={OperationKey} after {Seconds}s",
                        context.OperationKey ?? operationKey, span.TotalSeconds);
                    return Task.CompletedTask;
                });

            // Fallback: Context-aware responses from shared contracts
            var fallback = Policy<HttpResponseMessage>
                .Handle<Exception>()
                .OrResult(r => !r.IsSuccessStatusCode)
                .FallbackAsync(
                    fallbackAction: (delegateResult, context, ct) =>
                    {
                        var fallbackResponse = FallbackResponses.GetFallback(operationKey);
                        
                        Log.Warning("[Fallback] op={OperationKey} returning fallback. StatusCode={StatusCode}, OriginalError={Error}",
                            operationKey,
                            fallbackResponse.StatusCode,
                            delegateResult.Exception?.Message ?? delegateResult.Result?.StatusCode.ToString() ?? "unknown");
                        
                        return Task.FromResult(fallbackResponse.ToHttpResponse());
                    },
                    onFallbackAsync: (delegateResult, context) =>
                    {
                        Log.Information("[Fallback] op={OperationKey} fallback triggered", operationKey);
                        return Task.CompletedTask;
                    });

            // Bulkhead: limit parallel calls to prevent thread starvation
            var bulkhead = Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization: 20,
                maxQueuingActions: 10,
                onBulkheadRejectedAsync: ctx =>
                {
                    Log.Warning("[Bulkhead] op={OperationKey} rejected due to overload",
                        ctx.OperationKey ?? operationKey);
                    return Task.CompletedTask;
                });

            // Wrap order (outer → inner): Bulkhead → Fallback → Breaker → Retry → Timeout
            return Policy
                .WrapAsync(
                    bulkhead,
                    Policy.WrapAsync(
                        fallback,
                        Policy.WrapAsync(
                            breaker,
                            Policy.WrapAsync(retry, timeout))))
                .WithPolicyKey(operationKey);
        }

        /// <summary>
        /// Determines if a status code should trigger a retry
        /// </summary>
        private static bool ShouldRetry(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.RequestTimeout => true,      // 408
                HttpStatusCode.TooManyRequests => true,     // 429
                HttpStatusCode.InternalServerError => true, // 500
                HttpStatusCode.BadGateway => true,          // 502
                HttpStatusCode.ServiceUnavailable => true,  // 503
                HttpStatusCode.GatewayTimeout => true,      // 504
                _ => false
            };
        }
    }
}

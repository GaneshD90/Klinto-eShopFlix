using Contracts.Resilience;
using Polly;
using Polly.Timeout;
using Serilog;
using System.Net;

namespace CatalogService.Api.Extensions
{
    /// <summary>
    /// Resilience pipeline for outbound HTTP calls with context-aware fallback responses.
    /// Uses shared fallback responses from Contracts for consistency across services.
    /// </summary>
    public static class PollyPolicies
    {
        /// <summary>
        /// Creates a policy with context-aware fallback response
        /// </summary>
        /// <param name="operationKey">Operation identifier for logging and fallback selection</param>
        public static IAsyncPolicy<HttpResponseMessage> CreatePolicy(string operationKey)
            => BuildCompositePolicy(operationKey);

        public static IAsyncPolicy<HttpResponseMessage> CreatePolicy()
            => BuildCompositePolicy("http");

        private static IAsyncPolicy<HttpResponseMessage> BuildCompositePolicy(string operationKey)
        {
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
                    {
                        var status = outcome.Result?.StatusCode.ToString() ?? outcome.Exception?.GetType().Name ?? "unknown";
                        Log.Warning("[Polly-Retry] op={OperationKey} attempt={Attempt} delay={Delay}s status={Status}",
                            ctx.OperationKey ?? operationKey,
                            attempt,
                            Math.Round(delay.TotalSeconds, 3),
                            status);
                    }
                );

            var breaker = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .Or<TaskCanceledException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, span) =>
                    {
                        var status = outcome.Result?.StatusCode.ToString() ?? outcome.Exception?.GetType().Name ?? "unknown";
                        Log.Warning("[CircuitBreaker] op={OperationKey} OPEN for {Break}s status={Status}",
                            operationKey, span.TotalSeconds, status);
                    },
                    onReset: () => Log.Information("[CircuitBreaker] op={OperationKey} RESET", operationKey),
                    onHalfOpen: () => Log.Information("[CircuitBreaker] op={OperationKey} HALF-OPEN", operationKey)
                );

            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(20),
                timeoutStrategy: TimeoutStrategy.Optimistic,
                onTimeoutAsync: (context, span, task, ct) =>
                {
                    Log.Error("[Timeout] op={OperationKey} after {Seconds}s",
                        context.OperationKey ?? operationKey, span.TotalSeconds);
                    return Task.CompletedTask;
                });

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

            var bulkhead = Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization: 20,
                maxQueuingActions: 10,
                onBulkheadRejectedAsync: ctx =>
                {
                    Log.Warning("[Bulkhead] op={OperationKey} rejected due to overload",
                        ctx.OperationKey ?? operationKey);
                    return Task.CompletedTask;
                });

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

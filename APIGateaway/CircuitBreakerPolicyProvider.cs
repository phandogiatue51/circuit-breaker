using Clients;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace APIGateaway
{
    public class CircuitBreakerPolicyProvider
    {
        public IAsyncPolicy<HttpResponseMessage> BrandPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> CategoryPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> ProductPolicy { get; }

        public CircuitBreakerPolicyProvider()
        {
            BrandPolicy = CreateSuperPipeline("BRAND-SERVICE",
                CircuitBreakerRegistry.BrandServiceManualControl,
                CircuitBreakerRegistry.BrandServiceStateProvider);

            CategoryPolicy = CreateSuperPipeline("CATEGORY-SERVICE",
                CircuitBreakerRegistry.CategoryServiceManualControl,
                CircuitBreakerRegistry.CategoryServiceStateProvider);

            ProductPolicy = CreateSuperPipeline("PRODUCT-SERVICE",
                CircuitBreakerRegistry.ProductServiceManualControl,
                CircuitBreakerRegistry.ProductServiceStateProvider);
        }

        private IAsyncPolicy<HttpResponseMessage> CreateSuperPipeline(
            string serviceName,
            CircuitBreakerManualControl manualControl,
            CircuitBreakerStateProvider stateProvider)
        {
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(2),
                TimeoutStrategy.Optimistic,
                (context, timeSpan, task) =>
                {
                    Console.WriteLine($"⏱️ TIMEOUT: {serviceName} request timed out after {timeSpan.TotalSeconds}s");
                    return Task.CompletedTask;
                });

            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .OrResult(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    3, 
                    retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt))
                                   + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 50)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"🔄 RETRY {retryCount}: {serviceName} - Waiting {timespan.TotalMilliseconds}ms");
                    });

            var circuitBreakerOptions = new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(15),
                ManualControl = manualControl,
                StateProvider = stateProvider,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(response => !response.IsSuccessStatusCode),

                OnOpened = (args) =>
                {
                    Console.WriteLine($"🔴 CIRCUIT OPENED: {serviceName} - Blocking for {args.BreakDuration.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                },
                OnClosed = (args) =>
                {
                    Console.WriteLine($"🟢 CIRCUIT CLOSED: {serviceName} - Healthy again");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = (args) =>
                {
                    Console.WriteLine($"🟡 CIRCUIT HALF-OPEN: {serviceName} - Testing the waters");
                    return ValueTask.CompletedTask;
                }
            };

            var circuitBreaker = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddCircuitBreaker(circuitBreakerOptions)
                .Build()
                .AsAsyncPolicy();

            var fallbackPolicy = Policy<HttpResponseMessage>
                .Handle<Exception>()
                .OrResult(r => !r.IsSuccessStatusCode)
                .FallbackAsync(
                    fallbackAction: async (context, token) =>
                    {
                        Console.WriteLine($"📋 FALLBACK: Using cached data for {serviceName}");

                        if (FallbackCache.GetCachedResponse(serviceName) is HttpResponseMessage cached)
                        {
                            var clone = new HttpResponseMessage(cached.StatusCode)
                            {
                                Content = await CloneContentAsync(cached.Content),
                                ReasonPhrase = cached.ReasonPhrase
                            };
                            return clone;
                        }

                        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                        {
                            Content = new StringContent($@"{{
                                ""service"": ""{serviceName}"",
                                ""status"": ""degraded"",
                                ""message"": ""Using fallback data"",
                                ""timestamp"": ""{DateTime.Now:O}""
                            }}")
                        };
                    },
                    onFallbackAsync: async (outcome, context) =>
                    {
                        Console.WriteLine($"⚠️ FALLBACK TRIGGERED: {serviceName} - Serving degraded response");
                    });

            return fallbackPolicy
                .WrapAsync(circuitBreaker)
                .WrapAsync(retryPolicy)
                .WrapAsync(timeoutPolicy);
        }

        private async Task<HttpContent> CloneContentAsync(HttpContent content)
        {
            if (content == null) return null;

            var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            ms.Position = 0;

            var clone = new StreamContent(ms);
            foreach (var header in content.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return clone;
        }
    }
}
using Clients;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using APIGateaway.Services;

namespace APIGateaway
{
    public class CircuitBreakerPolicyProvider
    {
        public IAsyncPolicy<HttpResponseMessage> BrandPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> CategoryPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> ProductPolicy { get; }

        private readonly ICacheService _cache;

        public CircuitBreakerPolicyProvider(ICacheService cache)
        {
            _cache = cache;

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
                    Console.WriteLine($"⏱TIMEOUT: {serviceName} request timed out after {timeSpan.TotalSeconds}s");
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
                    onRetry: async (outcome, timespan, retryCount, context) =>
                    {
                        // Cache successful responses with the cache key from context
                        if (outcome.Result?.IsSuccessStatusCode == true && context.ContainsKey("CacheKey"))
                        {
                            var cacheKey = context["CacheKey"] as string;
                            if (!string.IsNullOrEmpty(cacheKey))
                            {
                                await _cache.SetCachedResponseAsync(serviceName, cacheKey, outcome.Result, TimeSpan.FromMinutes(5));
                            }
                        }
                        Console.WriteLine($"RETRY {retryCount}: {serviceName} - Waiting {timespan.TotalMilliseconds}ms");
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
                    Console.WriteLine("======================================================");
                    Console.WriteLine("======================================================");
                    Console.WriteLine($"CIRCUIT OPENED: {serviceName} - Blocking for {args.BreakDuration.TotalSeconds:F0} seconds");
                    Console.WriteLine("======================================================");
                    Console.WriteLine("======================================================");
                    return ValueTask.CompletedTask;
                },
                OnClosed = (args) =>
                {
                    Console.WriteLine("======================================================");
                    Console.WriteLine("======================================================");
                    Console.WriteLine($"CIRCUIT CLOSED: {serviceName} - Healthy again");
                    Console.WriteLine("======================================================");
                    Console.WriteLine("======================================================");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = (args) =>
                {
                    Console.WriteLine("======================================================");
                    Console.WriteLine("======================================================");
                    Console.WriteLine($"CIRCUIT HALF-OPEN: {serviceName} - Testing the waters");
                    Console.WriteLine("======================================================");
                    Console.WriteLine("======================================================");
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
                        Console.WriteLine($"FALLBACK: Using cached data for {serviceName}");

                        // Try to get from real cache using the cache key from context
                        if (context.ContainsKey("CacheKey"))
                        {
                            var cacheKey = context["CacheKey"] as string;
                            if (!string.IsNullOrEmpty(cacheKey))
                            {
                                var cached = await _cache.GetCachedResponseAsync(serviceName, cacheKey);
                                if (cached != null)
                                {
                                    var clone = new HttpResponseMessage(cached.StatusCode)
                                    {
                                        Content = await CloneContentAsync(cached.Content),
                                        ReasonPhrase = cached.ReasonPhrase
                                    };
                                    return clone;
                                }
                            }
                        }

                        // If no cache, return degraded response
                        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                        {
                            Content = new StringContent($@"{{
                                ""service"": ""{serviceName}"",
                                ""status"": ""degraded"",
                                ""message"": ""Service unavailable, no cached data"",
                                ""timestamp"": ""{DateTime.Now:O}""
                            }}")
                        };
                    },
                    onFallbackAsync: async (outcome, context) =>
                    {
                        Console.WriteLine($"FALLBACK TRIGGERED: {serviceName} - Serving degraded response");
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
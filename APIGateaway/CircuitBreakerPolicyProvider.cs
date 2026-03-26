using APIGateaway.Services;
using Clients;
using Grpc.Net.Client.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using System.Net;

namespace APIGateaway
{
    public class CircuitBreakerPolicyProvider
    {
        public IAsyncPolicy<HttpResponseMessage> BrandPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> CategoryPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> ProductPolicy { get; }

        private readonly ICacheService _cache;
        private readonly ILogger<CircuitBreakerPolicyProvider> _logger;


        public CircuitBreakerPolicyProvider(ICacheService cache, ILogger<CircuitBreakerPolicyProvider> logger)
        {
            _cache = cache;
            _logger = logger;

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
            // 1. Timeout Pipeline
            //var timeoutPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            //    .AddTimeout(TimeSpan.FromSeconds(1))
            //    .Build();
            //var timeoutPolicy = timeoutPipeline.AsAsyncPolicy();

            //// 2. Retry Pipeline - Polly V8
            //var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            //    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            //    {
            //        MaxRetryAttempts = 2,
            //        Delay = TimeSpan.FromSeconds(1),
            //        BackoffType = DelayBackoffType.Constant,
            //        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            //            .Handle<HttpRequestException>()
            //            .Handle<TimeoutRejectedException>()
            //            .HandleResult(response => (int)response.StatusCode >= 500),
            //        OnRetry = args =>
            //        {
            //            // Access context properly in Polly V8
            //            if (args.Context.Properties.TryGetValue(new ResiliencePropertyKey<string>("CacheKey"), out var cacheKey))
            //            {
            //                if (!string.IsNullOrEmpty(cacheKey) && args.Outcome.Result?.IsSuccessStatusCode == true)
            //                {
            //                    // Fire and forget caching (or await properly if needed)
            //                    _ = _cache.SetCachedResponseAsync(serviceName, cacheKey, args.Outcome.Result, TimeSpan.FromMinutes(5));
            //                }
            //            }

            //            _logger.LogInformation(
            //                "RETRY {AttemptNumber} for {ServiceName} - Waiting {Delay}ms - Reason: {Reason}",
            //                args.AttemptNumber,
            //                serviceName,
            //                args.RetryDelay.TotalMilliseconds,
            //                args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()
            //            );

            //            return default;
            //        }
            //    })
            //    .Build();
            //var retryPolicy = retryPipeline.AsAsyncPolicy();

            // 3. Circuit Breaker Pipeline - Polly V8
            var circuitBreakerOptions = new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(20),
                ManualControl = manualControl,
                StateProvider = stateProvider,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(response => (int)response.StatusCode >= 500),
                OnOpened = args =>
                {
                    Console.WriteLine("=============================================");
                    Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine("CIRCUIT BREAKER OPENED!");
                    Console.WriteLine($"Will stay open for {args.BreakDuration.TotalSeconds} seconds");
                    Console.WriteLine("=============================================");
                    return default;
                },
                OnClosed = args =>
                {
                    Console.WriteLine("=============================================");
                    Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine("CIRCUIT BREAKER CLOSED!");
                    Console.WriteLine("=============================================");
                    return default;
                },
                OnHalfOpened = args =>
                {
                    Console.WriteLine("=============================================");
                    Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine("CIRCUIT HALF-OPEN!");
                    Console.WriteLine("Testing if service is healthy...");
                    Console.WriteLine("=============================================");
                    return default;
                }
            };

            var circuitBreakerPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddCircuitBreaker(circuitBreakerOptions)
                .Build();
            var circuitBreakerPolicy = circuitBreakerPipeline.AsAsyncPolicy();

            // 4. Fallback Pipeline - Polly V8
            var fallbackPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutRejectedException>()
                        .HandleResult(response => (int)response.StatusCode >= 500),
                    OnFallback = args =>
                    {
                        _logger.LogWarning("FALLBACK TRIGGERED: {ServiceName} - Serving degraded response", serviceName);
                        return default;
                    },
                    FallbackAction = async args =>
                    {
                        _logger.LogInformation("FALLBACK: Attempting to use cached data for {ServiceName}", serviceName);

                        // Access context properly in Polly V8
                        if (args.Context.Properties.TryGetValue(new ResiliencePropertyKey<string>("CacheKey"), out var cacheKey))
                        {
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
                                    return Outcome.FromResult(clone);
                                }
                            }
                        }

                        // If no cache, return degraded response
                        var degradedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent($@"{{
                                ""service"": ""{serviceName}"",
                                ""status"": ""degraded"",
                                ""message"": ""Service unavailable, no cached data"",
                                ""timestamp"": ""{DateTime.Now:O}""
                            }}")
                        };
                        return Outcome.FromResult(degradedResponse);
                    }
                })
                .Build();
            var fallbackPolicy = fallbackPipeline.AsAsyncPolicy();

            //return circuitBreakerPolicy
            //    .WrapAsync(
            //        fallbackPolicy.WrapAsync(
            //            retryPolicy.WrapAsync(timeoutPolicy)
            //        )
            //    );
            return circuitBreakerPolicy;
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
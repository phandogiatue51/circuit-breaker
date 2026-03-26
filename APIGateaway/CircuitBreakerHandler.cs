using APIGateaway.Services;
using Polly;
using Polly.CircuitBreaker;
using System.Net;
using System.Text;
using System.Text.Json;

namespace APIGateaway
{
    public class CircuitBreakerHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;
        private readonly string _serviceName;
        private readonly ICacheService _cache;

        public CircuitBreakerHandler(IAsyncPolicy<HttpResponseMessage> policy, string serviceName, ICacheService cache)
        {
            _policy = policy;
            _serviceName = serviceName;
            _cache = cache;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
        {
            // Create cache key from the request URL
            var cacheKey = $"{request.Method}:{request.RequestUri?.PathAndQuery}";

            // Create context and add the cache key so it can be accessed by retry and fallback policies
            var context = new Context($"{_serviceName}-{Guid.NewGuid()}")
            {
                ["CacheKey"] = cacheKey  // ← THIS IS THE IMPORTANT PART
            };

            try
            {
                return await _policy.ExecuteAsync(async (ctx) =>
                {
                    var requestClone = await CloneHttpRequestMessageAsync(request);
                    var response = await base.SendAsync(requestClone, cancellationToken);

                    // If successful, cache the response with the URL as key
                    if (response.IsSuccessStatusCode)
                    {
                        await _cache.SetCachedResponseAsync(_serviceName, cacheKey, response, TimeSpan.FromMinutes(5));
                        Console.WriteLine($"✅ CACHE SET: {_serviceName} - {cacheKey}");
                    }

                    return response;
                }, context);  // ← Pass the context with cacheKey
            }
            catch (BrokenCircuitException)
            {
                var cachedResponse = await _cache.GetCachedResponseAsync(_serviceName, cacheKey);

                if (cachedResponse != null)
                {
                    Console.WriteLine($"💾 CACHE HIT: {_serviceName} - {cacheKey}");
                    return cachedResponse;
                }

                Console.WriteLine($"❌ CACHE MISS: {_serviceName} - {cacheKey}");

                var payload = new
                {
                    service = _serviceName,
                    status = "unavailable",
                    message = "Service temporarily unavailable and no cached data found",
                    cacheKey = cacheKey,
                    timestamp = DateTime.UtcNow.ToString("O")
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                response.Headers.Add("Access-Control-Allow-Origin", "*");

                return response;
            }
        }

        private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Clone Options for .NET Core 6.0+
            foreach (var option in request.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);
            }

            return clone;
        }
    }

    public class BrandCircuitBreakerHandler : CircuitBreakerHandler
    {
        public BrandCircuitBreakerHandler(CircuitBreakerPolicyProvider policyProvider, ICacheService cache)
            : base(policyProvider.BrandPolicy, "BRAND-SERVICE", cache) // ← Added cache parameter
        {
        }
    }

    public class CategoryCircuitBreakerHandler : CircuitBreakerHandler
    {
        public CategoryCircuitBreakerHandler(CircuitBreakerPolicyProvider policyProvider, ICacheService cache)
            : base(policyProvider.CategoryPolicy, "CATEGORY-SERVICE", cache) 
        {
        }
    }

    public class ProductCircuitBreakerHandler : CircuitBreakerHandler
    {
        public ProductCircuitBreakerHandler(CircuitBreakerPolicyProvider policyProvider, ICacheService cache)
            : base(policyProvider.ProductPolicy, "PRODUCT-SERVICE", cache) 
        {
        }
    }
}
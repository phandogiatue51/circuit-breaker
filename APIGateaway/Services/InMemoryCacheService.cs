using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace APIGateaway.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private const string AllowedOrigin = "http://localhost:5173";

        public InMemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<HttpResponseMessage?> GetCachedResponseAsync(string serviceName, string cacheKey)
        {
            var key = $"{serviceName}:{cacheKey}";
            if (_cache.TryGetValue(key, out HttpResponseMessage? cached) && cached != null)
            {
                // Add CORS headers to the cached response before returning
                var responseWithCors = await CloneResponseWithCorsAsync(cached);
                return responseWithCors;
            }
            return null;
        }

        public async Task SetCachedResponseAsync(string serviceName, string cacheKey, HttpResponseMessage response, TimeSpan ttl)
        {
            var key = $"{serviceName}:{cacheKey}";
            // Add CORS headers to the response before caching
            var clonedResponse = await CloneResponseWithCorsAsync(response);
            _cache.Set(key, clonedResponse, ttl);
        }

        private async Task<HttpResponseMessage> CloneResponseWithCorsAsync(HttpResponseMessage response)
        {
            var clone = new HttpResponseMessage(response.StatusCode)
            {
                ReasonPhrase = response.ReasonPhrase
            };

            // Copy existing headers
            foreach (var header in response.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Add CORS headers (override if they exist)
            clone.Headers.Remove("Access-Control-Allow-Origin");
            clone.Headers.Add("Access-Control-Allow-Origin", AllowedOrigin);
            clone.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH");
            clone.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
            clone.Headers.Add("Access-Control-Allow-Credentials", "true");
            clone.Headers.Add("Access-Control-Max-Age", "86400");

            // Copy content
            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
                clone.Content = new StringContent(content, Encoding.UTF8, contentType);

                // Copy other content headers if needed
                foreach (var header in response.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }
    }
}
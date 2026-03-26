// APIGateaway/Services/InMemoryCacheService.cs
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace APIGateaway.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public InMemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<HttpResponseMessage?> GetCachedResponseAsync(string serviceName, string cacheKey)
        {
            var key = $"{serviceName}:{cacheKey}";
            _cache.TryGetValue(key, out HttpResponseMessage? cached);
            return Task.FromResult(cached);
        }

        public async Task SetCachedResponseAsync(string serviceName, string cacheKey, HttpResponseMessage response, TimeSpan ttl)
        {
            var key = $"{serviceName}:{cacheKey}";
            var clonedResponse = await CloneResponseAsync(response);
            _cache.Set(key, clonedResponse, ttl);
        }

        private async Task<HttpResponseMessage> CloneResponseAsync(HttpResponseMessage response)
        {
            var clone = new HttpResponseMessage(response.StatusCode)
            {
                ReasonPhrase = response.ReasonPhrase
            };

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync();
                clone.Content = new StringContent(content, Encoding.UTF8, response.Content.Headers.ContentType?.MediaType);
            }

            foreach (var header in response.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
using System.Text.Json;

namespace APIGateaway
{
    public static class FallbackCache
    {
        private static readonly Dictionary<string, HttpResponseMessage> _cache = new();

        public static HttpResponseMessage GetCachedResponse(string serviceName)
        {
            if (_cache.TryGetValue(serviceName, out var cached))
            {
                Console.WriteLine($"✅ FALLBACK: Returning cached response for {serviceName}");
                return cached;
            }

            var fallback = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    message = $"{serviceName} is currently unavailable. Showing cached data.",
                    timestamp = DateTime.Now,
                    data = new[] { "cached item 1", "cached item 2" }
                }))
            };

            _cache[serviceName] = fallback;
            return fallback;
        }

        public static void UpdateCache(string serviceName, HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                _cache[serviceName] = response;
                Console.WriteLine($"📦 CACHE UPDATED: Fresh data for {serviceName}");
            }
        }
    }
}
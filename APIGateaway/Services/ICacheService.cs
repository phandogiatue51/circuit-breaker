namespace APIGateaway.Services
{
    public interface ICacheService
    {
        Task<HttpResponseMessage?> GetCachedResponseAsync(string serviceName, string cacheKey);
        Task SetCachedResponseAsync(string serviceName, string cacheKey, HttpResponseMessage response, TimeSpan ttl);
    }
}

using Clients;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace APIGateaway.Controllers
{
    [ApiController]
    [Route("internal/[controller]")]
    public class ResilienceDemoController : ControllerBase
    {
        private readonly CircuitBreakerPolicyProvider _policies;

        public ResilienceDemoController(CircuitBreakerPolicyProvider policies)
        {
            _policies = policies;
        }

        [HttpPost("test/brand")]
        public async Task<IActionResult> TestBrandPipeline()
        {
            var results = new List<string>();

            for (int i = 1; i <= 10; i++)
            {
                try
                {
                    var response = await _policies.BrandPolicy.ExecuteAsync(async () =>
                    {
                        var client = new HttpClient();
                        var result = await client.GetAsync("https://localhost:7197/api/brands/999");
                        return result;
                    });

                    results.Add($"Request {i}: Success - {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    results.Add($"Request {i}: Failed - {ex.Message}");
                }
            }

            return Ok(new
            {
                Message = "Resilience pipeline test completed",
                CircuitState = CircuitBreakerRegistry.BrandServiceStateProvider.CircuitState.ToString(),
                Results = results
            });
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            return Ok(new
            {
                Brand = new
                {
                    State = CircuitBreakerRegistry.BrandServiceStateProvider.CircuitState.ToString(),
                    ManualControl = CircuitBreakerRegistry.BrandServiceManualControl != null
                },
                CacheStats = new
                {
                    BrandCacheExists = FallbackCache.GetCachedResponse("BRAND-SERVICE") != null
                }
            });
        }
    }
}
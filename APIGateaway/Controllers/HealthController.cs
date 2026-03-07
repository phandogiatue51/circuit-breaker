using Clients;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace APIGateaway.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            var brandState = CircuitBreakerRegistry.BrandServiceStateProvider.CircuitState;
            var categoryState = CircuitBreakerRegistry.CategoryServiceStateProvider.CircuitState;

            var healthStatus = new
            {
                Overall = (brandState == CircuitState.Closed && categoryState == CircuitState.Closed)
                    ? "Healthy"
                    : "Degraded",
                Circuits = new[]
                {
                new
                {
                    Service = "Brand Service",
                    State = brandState.ToString(),
                    IsAvailable = brandState == CircuitState.Closed,
                    IsManuallyIsolated = brandState == CircuitState.Isolated,
                    CanAutoRecover = brandState != CircuitState.Isolated
                },
                new
                {
                    Service = "Category Service",
                    State = categoryState.ToString(),
                    IsAvailable = categoryState == CircuitState.Closed,
                    IsManuallyIsolated = categoryState == CircuitState.Isolated,
                    CanAutoRecover = categoryState != CircuitState.Isolated
                }
            }
            };

            return Ok(healthStatus);
        }
    }
}

using Clients;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace APIGateaway.Controllers
{
    [ApiController]
    [Route("internal/[controller]")]
    public class CircuitBreakerController : ControllerBase
    {
        private readonly BrandServiceClient _brandClient;
        private readonly CategoryServiceClient _categoryClient;
        private readonly ILogger<CircuitBreakerController> _logger;

        public CircuitBreakerController(
            BrandServiceClient brandClient,
            CategoryServiceClient categoryClient,
            ILogger<CircuitBreakerController> logger)
        {
            _brandClient = brandClient;
            _categoryClient = categoryClient;
            _logger = logger;
        }

        [HttpPost("isolate/brand")]
        public async Task<IActionResult> IsolateBrandService()
        {
            await CircuitBreakerRegistry.BrandServiceManualControl.IsolateAsync();

            var finalState = CircuitBreakerRegistry.BrandServiceStateProvider.CircuitState.ToString();

            return Ok(new
            {
                Message = "Brand service circuit isolation attempted",
                ManualControlSet = "Isolated",
                FinalState = finalState
            });
        }

        [HttpPost("close/brand")]
        public async Task<IActionResult> CloseBrandService()
        {
            await CircuitBreakerRegistry.BrandServiceManualControl.CloseAsync();

            var finalState = CircuitBreakerRegistry.BrandServiceStateProvider.CircuitState.ToString();

            return Ok(new
            {
                Message = "Brand service circuit close attempted",
                ManualControlSet = "Closed",
                FinalState = finalState
            });
        }

        [HttpPost("isolate/category")]
        public async Task<IActionResult> IsolateCategoryService()
        {
            await CircuitBreakerRegistry.CategoryServiceManualControl.IsolateAsync();


            var finalState = CircuitBreakerRegistry.CategoryServiceStateProvider.CircuitState.ToString();

            return Ok(new
            {
                Message = "Category service circuit isolation attempted",
                ManualControlSet = "Isolated",
                FinalState = finalState
            });
        }

        [HttpPost("close/category")]
        public async Task<IActionResult> CloseCategoryService()
        {
            await CircuitBreakerRegistry.CategoryServiceManualControl.CloseAsync();

            var finalState = CircuitBreakerRegistry.CategoryServiceStateProvider.CircuitState.ToString();

            return Ok(new
            {
                Message = "Category service circuit close attempted",
                ManualControlSet = "Closed",
                FinalState = finalState
            });
        }

        [HttpPost("isolate/product")]
        public async Task<IActionResult> IsolateProductService()
        {
            await CircuitBreakerRegistry.ProductServiceManualControl.IsolateAsync();

            var finalState = CircuitBreakerRegistry.ProductServiceStateProvider.CircuitState.ToString();

            return Ok(new
            {
                Message = "Product service circuit isolation attempted",
                ManualControlSet = "Isolated",
                FinalState = finalState
            });
        }

        [HttpPost("close/product")]
        public async Task<IActionResult> CloseProductService()
        {
            await CircuitBreakerRegistry.ProductServiceManualControl.CloseAsync();

            var finalState = CircuitBreakerRegistry.ProductServiceStateProvider.CircuitState.ToString();

            return Ok(new
            {
                Message = "Product service circuit close attempted",
                ManualControlSet = "Closed",
                FinalState = finalState
            });
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("CircuitBreaker controller is working!");
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                BrandService = new
                {
                    State = CircuitBreakerRegistry.BrandServiceStateProvider.CircuitState.ToString(),
                    IsManuallyControlled = true,
                    ManualControlHash = CircuitBreakerRegistry.BrandServiceManualControl.GetHashCode()
                },
                CategoryService = new
                {
                    State = CircuitBreakerRegistry.CategoryServiceStateProvider.CircuitState.ToString(),
                    IsManuallyControlled = true,
                    ManualControlHash = CircuitBreakerRegistry.CategoryServiceManualControl.GetHashCode()
                },
                ProductService = new
                {
                    State = CircuitBreakerRegistry.ProductServiceStateProvider.CircuitState.ToString(),
                    IsManuallyControlled = true,
                    ManualControlHash = CircuitBreakerRegistry.ProductServiceManualControl.GetHashCode()
                }
            });
        }

        [HttpGet("debug")]
        public IActionResult Debug()
        {
            return Ok(new
            {
                Brand = new
                {
                    ManualControlExists = CircuitBreakerRegistry.BrandServiceManualControl != null,
                    ManualControlHash = CircuitBreakerRegistry.BrandServiceManualControl?.GetHashCode(),
                    StateProviderExists = CircuitBreakerRegistry.BrandServiceStateProvider != null,
                    CurrentState = CircuitBreakerRegistry.BrandServiceStateProvider?.CircuitState.ToString() ?? "null"
                },
                Category = new
                {
                    ManualControlExists = CircuitBreakerRegistry.CategoryServiceManualControl != null,
                    ManualControlHash = CircuitBreakerRegistry.CategoryServiceManualControl?.GetHashCode(),
                    StateProviderExists = CircuitBreakerRegistry.CategoryServiceStateProvider != null,
                    CurrentState = CircuitBreakerRegistry.CategoryServiceStateProvider?.CircuitState.ToString() ?? "null"
                },
                Product = new
                {
                    ManualControlExists = CircuitBreakerRegistry.ProductServiceManualControl != null,
                    ManualControlHash = CircuitBreakerRegistry.ProductServiceManualControl?.GetHashCode(),
                    StateProviderExists = CircuitBreakerRegistry.ProductServiceStateProvider != null,
                    CurrentState = CircuitBreakerRegistry.ProductServiceStateProvider?.CircuitState.ToString() ?? "null"
                }
            });
        }
    }
}

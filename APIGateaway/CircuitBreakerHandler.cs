using Polly;
using Polly.CircuitBreaker;

namespace APIGateaway
{
    public class CircuitBreakerHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;
        private readonly string _serviceName;

        public CircuitBreakerHandler(IAsyncPolicy<HttpResponseMessage> policy, string serviceName)
        {
            _policy = policy;
            _serviceName = serviceName;
            Console.WriteLine($"CircuitBreakerHandler CREATED for {serviceName} at {DateTime.Now:HH:mm:ss}");
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] CircuitBreakerHandler EXECUTING for {_serviceName}");
            Console.WriteLine($"Request URL: {request.RequestUri}");
            Console.WriteLine($"Policy HashCode: {_policy.GetHashCode()}");

            try
            {
                return await _policy.ExecuteAsync(async (context) =>
                {
                    Console.WriteLine($"Executing request inside circuit breaker for {_serviceName}");
                    var response = await base.SendAsync(request, cancellationToken);
                    Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
                    return response;
                }, new Context($"{_serviceName}-{Guid.NewGuid()}"));
            }
            catch (BrokenCircuitException ex)
            {
                Console.WriteLine($"CIRCUIT BREAKER OPEN - Request blocked for {_serviceName}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent($"Circuit breaker is open for {_serviceName}. Service temporarily unavailable.")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in circuit breaker: {ex.Message}");
                throw;
            }
        }
    }

    public class BrandCircuitBreakerHandler : CircuitBreakerHandler
    {
        public BrandCircuitBreakerHandler(CircuitBreakerPolicyProvider policyProvider)
            : base(policyProvider.BrandPolicy, "BRAND-SERVICE")
        {
            Console.WriteLine("BrandCircuitBreakerHandler CREATED");
        }
    }

    public class CategoryCircuitBreakerHandler : CircuitBreakerHandler
    {
        public CategoryCircuitBreakerHandler(CircuitBreakerPolicyProvider policyProvider)
            : base(policyProvider.CategoryPolicy, "CATEGORY-SERVICE")
        {
            Console.WriteLine("CategoryCircuitBreakerHandler CREATED");
        }
    }

    public class ProductCircuitBreakerHandler : CircuitBreakerHandler
    {
        public ProductCircuitBreakerHandler(CircuitBreakerPolicyProvider policyProvider)
            : base(policyProvider.ProductPolicy, "PRODUCT-SERVICE")
        {
            Console.WriteLine("ProductCircuitBreakerHandler CREATED");
        }
    }
}
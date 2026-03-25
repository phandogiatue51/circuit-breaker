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
                    // CẦN PHẢI CLONE REQUEST, nếu không RetryPolicy sẽ văng lỗi "The request message was already sent" trong lần thử thứ 2
                    var requestClone = await CloneHttpRequestMessageAsync(request);
                    var response = await base.SendAsync(requestClone, cancellationToken);
                    
                    Console.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
                    return response;
                }, new Context($"{_serviceName}-{Guid.NewGuid()}"));
            }
            catch (BrokenCircuitException)
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
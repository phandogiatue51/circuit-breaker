using Clients;
using Polly;
using Polly.CircuitBreaker;

namespace APIGateaway
{
    public class CircuitBreakerPolicyProvider
    {
        public IAsyncPolicy<HttpResponseMessage> BrandPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> CategoryPolicy { get; }
        public IAsyncPolicy<HttpResponseMessage> ProductPolicy { get; }

        public CircuitBreakerPolicyProvider()
        {
            BrandPolicy = CreatePolicy("BRAND-SERVICE",
                CircuitBreakerRegistry.BrandServiceManualControl,
                CircuitBreakerRegistry.BrandServiceStateProvider);

            CategoryPolicy = CreatePolicy("CATEGORY-SERVICE",
                CircuitBreakerRegistry.CategoryServiceManualControl,
                CircuitBreakerRegistry.CategoryServiceStateProvider);

            ProductPolicy = CreatePolicy("PRODUCT-SERVICE",
                CircuitBreakerRegistry.ProductServiceManualControl,
                CircuitBreakerRegistry.ProductServiceStateProvider);
        }

        private IAsyncPolicy<HttpResponseMessage> CreatePolicy(
            string serviceName,
            CircuitBreakerManualControl manualControl,
            CircuitBreakerStateProvider stateProvider)
        {
            var options = new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(15),

                ManualControl = manualControl,
                StateProvider = stateProvider,

                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .HandleResult(response => !response.IsSuccessStatusCode),

                OnOpened = (args) =>
                {
                    Console.WriteLine($"=============================================");
                    Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine($"{serviceName} CIRCUIT OPENED");
                    Console.WriteLine($"Break duration: {args.BreakDuration}");
                    Console.WriteLine($"=============================================");
                    return ValueTask.CompletedTask;
                },

                OnClosed = (args) =>
                {
                    Console.WriteLine($"=============================================");
                    Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine($"{serviceName} CIRCUIT CLOSED");
                    Console.WriteLine($"=============================================");
                    return ValueTask.CompletedTask;
                },

                OnHalfOpened = (args) =>
                {
                    Console.WriteLine($"=============================================");
                    Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine($"{serviceName} CIRCUIT HALF-OPEN");
                    Console.WriteLine($"=============================================");
                    return ValueTask.CompletedTask;
                }
            };

            return new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddCircuitBreaker(options)
                .Build()
                .AsAsyncPolicy();
        }
    }
}
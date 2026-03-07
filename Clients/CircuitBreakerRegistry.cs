using Polly.CircuitBreaker;

namespace Clients
{
    public static class CircuitBreakerRegistry
    {
        public static CircuitBreakerManualControl BrandServiceManualControl { get; } = new();
        public static CircuitBreakerStateProvider BrandServiceStateProvider { get; } = new();

        public static CircuitBreakerManualControl CategoryServiceManualControl { get; } = new();
        public static CircuitBreakerStateProvider CategoryServiceStateProvider { get; } = new();

        public static CircuitBreakerManualControl ProductServiceManualControl { get; } = new();
        public static CircuitBreakerStateProvider ProductServiceStateProvider { get; } = new();

        public static CircuitBreakerManualControl AuthServiceManualControl { get; } = new();
        public static CircuitBreakerStateProvider AuthServiceStateProvider { get; } = new();
    }
}
namespace DTOs.Exceptions;

public class CircuitBreakerOpenException : BaseException
{
    public CircuitBreakerOpenException(string serviceName)
        : base($"Service {serviceName} hiện không khả dụng. Vui lòng thử lại sau.", 503, "CIRCUIT_BREAKER_OPEN")
    {
    }
}
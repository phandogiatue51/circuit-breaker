# ⚡ Circuit Breaker in ASP.NET Core API with API Gateway

## 📖 Overview
Circuit Breaker is a **resilience pattern** that prevents cascading failures in distributed systems. In ASP.NET Core APIs, especially when fronted by an **API Gateway**, Circuit Breakers help ensure that failing downstream services don’t overwhelm the system.

This README explains:
- Basic and advanced Circuit Breaker configurations using **Polly**
- Integration with API Gateway
- Monitoring and observability
- Best practices for production

---

## 🚀 Why Circuit Breaker?
- **Fail fast** when a service is down
- **Prevent resource exhaustion** from repeated retries
- **Graceful degradation** with fallbacks
- **Improve resilience** in microservices behind an API Gateway

---

## 🔧 Setup in ASP.NET Core

### 1. Install Polly
```bash
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

### 2. Configure Circuit Breaker in `Program.cs`
```csharp
builder.Services.AddHttpClient("DownstreamService")
    .AddResilienceHandler("circuit-breaker", (pipelineBuilder, context) =>
    {
        pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5, // Break when 50% of calls fail
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDuration = TimeSpan.FromSeconds(30)
        });
    });
```

---

## ⚙️ Advanced Features

### 🔹 Advanced Circuit Breaker
- **Failure Ratio**: Trips when failure percentage exceeds threshold
- **Sampling Duration**: Rolling time window
- **Minimum Throughput**: Ensures enough data before tripping

### 🔹 Manual Isolation (4th State)
```csharp
var manualControl = new CircuitBreakerManualControl();
await manualControl.IsolateAsync(); // Force open
await manualControl.CloseAsync();   // Restore
```

### 🔹 Dynamic Break Durations
```csharp
BreakDurationGenerator = args =>
    new ValueTask<TimeSpan>(TimeSpan.FromSeconds(Math.Min(300, args.FailureCount * 10)));
```

---

## 🏥 Health Checks & Monitoring
Expose circuit state via ASP.NET Core Health Checks:

```csharp
var stateProvider = new CircuitBreakerStateProvider();
builder.Services.AddHealthChecks()
    .AddCheck("circuit-breaker", () =>
        stateProvider.CircuitState == CircuitState.Closed
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Degraded());
```

Metrics to track:
- Circuit open frequency
- Duration of open states
- Blocked vs allowed requests

---

## 🔗 API Gateway Integration
When using **Ocelot** or another API Gateway:
- Apply Circuit Breaker policies at the **gateway level** for centralized control
- Optionally use **Redis** or distributed cache to share circuit state across multiple API instances
- Service meshes (e.g., Istio, Linkerd) can enforce circuit breaking at the network layer

---

## 🛡️ Combining Resilience Patterns
Circuit Breaker works best when combined with:
- **Timeouts** → Fail fast
- **Retries** → Handle transient errors
- **Fallbacks** → Return cached/default data
- **Bulkheads** → Isolate failures

Example pipeline:
```csharp
pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(2));
pipelineBuilder.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2 });
pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions { FailureRatio = 0.2 });
pipelineBuilder.AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
{
    FallbackAction = args => ValueTask.FromResult(Result.Success(GetCachedResponse()))
});
```

---

## 📊 Observability
Polly v8+ integrates with **OpenTelemetry**:
- Events: `OnCircuitOpened`, `OnCircuitHalfOpened`, `OnCircuitClosed`
- Severity levels: Error, Warning, Information
- Logs include pipeline and exception details

---

## ✅ Best Practices
- Tune thresholds based on traffic volume
- Use **distributed state** for multi-instance APIs
- Monitor and alert on circuit breaker events
- Combine with retries, timeouts, and fallbacks
- Avoid over-aggressive thresholds (can cause false trips)

---

## 🎯 Key Takeaway
Circuit Breakers in ASP.NET Core APIs (with API Gateway) are not just about blocking failures—they enable **intelligent failure detection**, **automatic recovery**, and **graceful degradation**. Combined with retries, timeouts, and fallbacks, they form the backbone of **cloud-native resilience**.
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.CircuitBreaker;
using Clients;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddHttpClient<BrandServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7246");
})
.AddPolicyHandler(GetCircuitBreakerPolicy(
    CircuitBreakerRegistry.BrandServiceManualControl,
    CircuitBreakerRegistry.BrandServiceStateProvider,
    "BRAND-SERVICE"
));

builder.Services.AddHttpClient<CategoryServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7246");
})
.AddPolicyHandler(GetCircuitBreakerPolicy(
    CircuitBreakerRegistry.CategoryServiceManualControl,
    CircuitBreakerRegistry.CategoryServiceStateProvider,
    "CATEGORY-SERVICE"
));

builder.Services.AddHttpClient<ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7246");
})
.AddPolicyHandler(GetCircuitBreakerPolicy(
    CircuitBreakerRegistry.ProductServiceManualControl,
    CircuitBreakerRegistry.ProductServiceStateProvider,
    "PRODUCT-SERVICE"
));

builder.Services.AddScoped<BrandServiceClient>();
builder.Services.AddScoped<CategoryServiceClient>();
builder.Services.AddScoped<ProductServiceClient>();

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
    CircuitBreakerManualControl manualControl,
    CircuitBreakerStateProvider stateProvider,
    string serviceName)
{
    Console.WriteLine($"=== CREATING POLICY FOR {serviceName} ===");
    Console.WriteLine($"ManualControl Hash received: {manualControl?.GetHashCode()}");
    Console.WriteLine($"StateProvider Hash received: {stateProvider?.GetHashCode()}");
    Console.WriteLine($"Expected Brand Hash: {CircuitBreakerRegistry.BrandServiceManualControl?.GetHashCode()}");
    Console.WriteLine($"Expected Category Hash: {CircuitBreakerRegistry.CategoryServiceManualControl?.GetHashCode()}");

    var options = new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        FailureRatio = 0.2,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 10,
        BreakDuration = TimeSpan.FromSeconds(15),

        ManualControl = manualControl,
        StateProvider = stateProvider,

        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutException>()
            .HandleResult(response => !response.IsSuccessStatusCode),

        OnOpened = (args) =>
        {
            Console.WriteLine("=============================================");
            Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
            Console.WriteLine($"🔴 {serviceName} CIRCUIT OPENED");
            Console.WriteLine($"IsManual: {args.IsManual}");
            Console.WriteLine($"ManualControl Hash in event: {manualControl?.GetHashCode()}");
            Console.WriteLine("=============================================");
            return ValueTask.CompletedTask;
        },

        OnClosed = (args) =>
        {
            Console.WriteLine($"✅ {serviceName} CIRCUIT CLOSED");
            return ValueTask.CompletedTask;
        },

        OnHalfOpened = (args) =>
        {
            Console.WriteLine($"⚠️ {serviceName} CIRCUIT HALF-OPEN");
            return ValueTask.CompletedTask;
        }
    };

    var policy = new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddCircuitBreaker(options)
        .Build()
        .AsAsyncPolicy();

    Console.WriteLine($"Policy created with ManualControl Hash: {manualControl?.GetHashCode()}");
    return policy;
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gateway Local APIs",
        Version = "v1",
        Description = "Health checks and Circuit Breaker management endpoints"
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway Local APIs v1");
    c.RoutePrefix = "swagger/local";
});

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.MapWhen(context =>
    context.Request.Path.StartsWithSegments("/brands") ||
    context.Request.Path.StartsWithSegments("/categories") ||
    context.Request.Path.StartsWithSegments("/products") ||
    context.Request.Path.StartsWithSegments("/auth"),
    appBuilder =>
    {
        appBuilder.UseOcelot().Wait();
    });

app.Run();
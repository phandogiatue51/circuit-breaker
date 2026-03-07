using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductService;
using System.Text;
using Polly;
using Polly.CircuitBreaker;
using Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<BrandServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7246");
})
.AddPolicyHandler(GetCircuitBreakerPolicy(
    CircuitBreakerRegistry.BrandServiceManualControl,
    CircuitBreakerRegistry.BrandServiceStateProvider,
    "BRAND-SERVICE-FROM-PRODUCT"
));

builder.Services.AddHttpClient<CategoryServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7246"); 
})
.AddPolicyHandler(GetCircuitBreakerPolicy(
    CircuitBreakerRegistry.CategoryServiceManualControl,
    CircuitBreakerRegistry.CategoryServiceStateProvider,
    "CATEGORY-SERVICE-FROM-PRODUCT"
));

builder.Services.AddScoped<BrandServiceClient>();
builder.Services.AddScoped<CategoryServiceClient>();

builder.Services.AddScoped<Repository>();
builder.Services.AddScoped<IService, Service>();

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
    CircuitBreakerManualControl manualControl,
    CircuitBreakerStateProvider stateProvider,
    string serviceName)
{
    var options = new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        MinimumThroughput = 3,
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(15),

        ManualControl = manualControl,
        StateProvider = stateProvider,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutException>()
            .HandleResult(response => !response.IsSuccessStatusCode)
    };

    return new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddCircuitBreaker(options)
        .Build()
        .AsAsyncPolicy();
}

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductService", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field. Example: \"Bearer {token}\"",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
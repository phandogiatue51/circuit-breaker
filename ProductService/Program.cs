using Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using ProductService;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 2,
            durationOfBreak: TimeSpan.FromSeconds(10),
            onBreak: (outcome, duration) =>
            {
                Console.WriteLine("=============================================");
                Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine("CIRCUIT BREAKER TRIPPED!");
                Console.WriteLine($"Duration: {duration.TotalSeconds} seconds");
                Console.WriteLine($"Reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                Console.WriteLine("=============================================");
            },
            onReset: () =>
            {
                Console.WriteLine("=============================================");
                Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine("CIRCUIT BREAKER RESET!");
                Console.WriteLine("=============================================");
            },
            onHalfOpen: () =>
            {
                Console.WriteLine("=============================================");
                Console.WriteLine($"TIME: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine("CIRCUIT HALF-OPEN");
                Console.WriteLine("=============================================");
            }
        );
}

builder.Services.AddHttpClient<BrandServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7197");
})
.AddPolicyHandler(GetCircuitBreakerPolicy()); 

builder.Services.AddHttpClient<CategoryServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7067");
})
.AddPolicyHandler(GetCircuitBreakerPolicy());

// SINGLE Swagger configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductService", Version = "v1" });

    // Add JWT Authentication support to Swagger
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

// Register repository and service
builder.Services.AddScoped<Repository>();
builder.Services.AddScoped<IService, Service>();

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
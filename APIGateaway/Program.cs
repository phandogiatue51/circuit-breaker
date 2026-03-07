using APIGateaway;
using Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddScoped<BrandServiceClient>();
builder.Services.AddScoped<CategoryServiceClient>();
builder.Services.AddScoped<ProductServiceClient>();
builder.Services.AddSingleton<CircuitBreakerPolicyProvider>();

builder.Services.AddTransient<BrandCircuitBreakerHandler>();
builder.Services.AddTransient<CategoryCircuitBreakerHandler>();
builder.Services.AddTransient<ProductCircuitBreakerHandler>();

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

builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<BrandCircuitBreakerHandler>()
    .AddDelegatingHandler<CategoryCircuitBreakerHandler>()
    .AddDelegatingHandler<ProductCircuitBreakerHandler>();

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

app.UseOcelot().Wait();

app.Run();
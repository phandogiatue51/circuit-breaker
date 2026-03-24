using DTOs;
using DTOs.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AuthService.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var path = httpContext.Request.Path.ToString();
        var (statusCode, message, errorCode) = exception switch
        {
            // Custom exceptions
            NotFoundException notFound => (notFound.StatusCode, notFound.Message, notFound.ErrorCode),
            BadRequestException badRequest => (badRequest.StatusCode, badRequest.Message, badRequest.ErrorCode),
            ConflictException conflict => (conflict.StatusCode, conflict.Message, conflict.ErrorCode),
            UnauthorizedException unauthorized => (unauthorized.StatusCode, unauthorized.Message, unauthorized.ErrorCode),
            ForbiddenException forbidden => (forbidden.StatusCode, forbidden.Message, forbidden.ErrorCode),
            CircuitBreakerOpenException circuitBreaker => (circuitBreaker.StatusCode, circuitBreaker.Message, circuitBreaker.ErrorCode),

            // Validation errors (FluentValidation)
            // FluentValidation.ValidationException validation => (400, validation.Message, "VALIDATION_ERROR"),

            // Database errors
            DbUpdateException dbUpdate => (409, "Dữ liệu bị trùng hoặc vi phạm ràng buộc", "DATABASE_CONFLICT"),

            // Default
            _ => (500, _environment.IsDevelopment() ? exception.Message : "Đã xảy ra lỗi hệ thống", "INTERNAL_SERVER_ERROR")
        };

        var response = new ApiResponse<object>
        {
            StatusCode = statusCode,
            Message = message,
            Data = null,
            Timestamp = DateTime.UtcNow,
            Path = path
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            cancellationToken);

        return true;
    }
}
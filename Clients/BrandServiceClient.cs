using DTOs;
using DTOs.Exceptions;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Net;
using System.Text.Json;

namespace Clients
{
    public class BrandServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BrandServiceClient> _logger;

        public BrandServiceClient(HttpClient httpClient, ILogger<BrandServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://localhost:7246");
        }

        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/queries/brands/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<BrandDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Brand with ID {BrandId} not found", id);
                    return null;
                }

                // Treat 5xx / service unavailable responses as service-level failures
                if ((int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    _logger.LogError("Downstream BrandService returned {StatusCode} for id {BrandId}", response.StatusCode, id);
                    // Throw an explicit circuit/service-unavailable exception so GlobalExceptionHandler produces 503
                    throw new CircuitBreakerOpenException("BRAND-SERVICE");
                }

                // For other non-success codes, log and treat as not found (or change behavior as needed)
                _logger.LogWarning("Unexpected status {StatusCode} from BrandService for id {BrandId}", response.StatusCode, id);
                return null;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Brand service circuit is OPEN or ISOLATED. Unable to call service for ID {BrandId}", id);
                throw new CircuitBreakerOpenException("BRAND-SERVICE");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling BrandService for ID {BrandId}", id);
                throw new CircuitBreakerOpenException("BRAND-SERVICE");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling BrandService for ID {BrandId}", id);
                throw;
            }
        }
    }
}
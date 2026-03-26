using DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Polly.CircuitBreaker;
using DTOs.Exceptions; 

namespace Clients
{
    public class ProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://localhost:7246");
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/queries/products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", id);
                    return null;
                }

                if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == System.Net.HttpStatusCode.BadGateway || response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                {
                    _logger.LogError("Downstream ProductService returned {StatusCode} for id {ProductId}", response.StatusCode, id);
                    throw new CircuitBreakerOpenException("PRODUCT-SERVICE");
                }

                return null;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Product service circuit is OPEN or ISOLATED. Unable to call service for ID {ProductId}", id);
                throw new Exception("Product service is currently unavailable (circuit open)", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling ProductService for ID {ProductId}", id);
                throw new Exception("Product service is unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ProductService for ID {ProductId}", id);
                throw;
            }
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/queries/products");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductDto>>>(content, options);

                    return apiResponse?.Data ?? new List<ProductDto>();
                }

                _logger.LogWarning("Failed to get all products");
                return new List<ProductDto>();
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Product service circuit is OPEN or ISOLATED. Unable to call service for GetAll");
                throw new Exception("Product service is currently unavailable (circuit open)", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllAsync");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var product = await GetByIdAsync(id);
                return product != null;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogWarning("Cannot check existence for product {ProductId} - circuit is open", id);
                throw;
            }
        }
    }
}
using DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Polly.CircuitBreaker;

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

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Brand with ID {BrandId} not found", id);
                    return null;
                }

                return null;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Brand service circuit is OPEN or ISOLATED. Unable to call service for ID {BrandId}", id);
                throw new Exception("Brand service is currently unavailable (circuit open)", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling BrandService for ID {BrandId}", id);
                throw new Exception("Brand service is unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling BrandService for ID {BrandId}", id);
                throw;
            }
        }
    }
}
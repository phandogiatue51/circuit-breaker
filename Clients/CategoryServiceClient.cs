using DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Polly.CircuitBreaker;

namespace Clients
{
    public class CategoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryServiceClient> _logger;

        public CategoryServiceClient(HttpClient httpClient, ILogger<CategoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://localhost:7246");
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/categories/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found", id);
                    return null;
                }

                return null;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Category service circuit is OPEN or ISOLATED. Unable to call service for ID {CategoryId}", id);
                throw new Exception("Category service is currently unavailable (circuit open)", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling CategoryService for ID {CategoryId}", id);
                throw new Exception("Category service is unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CategoryService for ID {CategoryId}", id);
                throw;
            }
        }

        public async Task<List<CategoryDto>> GetByIdsAsync(List<int> ids)
        {
            try
            {
                var url = $"/categories/by-ids?ids={string.Join("&ids=", ids)}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(content, options);

                    return apiResponse?.Data ?? new List<CategoryDto>();
                }

                _logger.LogWarning("Failed to get categories by IDs: {Ids}", string.Join(",", ids));
                return new List<CategoryDto>();
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Category service circuit is OPEN or ISOLATED. Unable to call service for IDs: {Ids}",
                    string.Join(",", ids));
                throw new Exception("Category service is currently unavailable (circuit open)", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetByIdsAsync for IDs: {Ids}", string.Join(",", ids));
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var category = await GetByIdAsync(id);
                return category != null;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogWarning("Cannot check existence for category {CategoryId} - circuit is open", id);
                throw;
            }
        }
    }
}
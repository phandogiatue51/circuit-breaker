using DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

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
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/categories/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return null;
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
                var idsQuery = string.Join("&ids=", ids);
                var url = $"/api/categories/by-ids?ids={idsQuery}";

                var response = await _httpClient.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(content, options);

                    if (apiResponse?.Data != null)
                    {
                        return apiResponse.Data;
                    }
                    else
                    {
                        _logger.LogWarning("apiResponse.Data is NULL! API Response: {@ApiResponse}", apiResponse);
                        return new List<CategoryDto>();
                    }
                }

                return new List<CategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetByIdsAsync for IDs: {Ids}", string.Join(",", ids));
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var category = await GetByIdAsync(id);
            return category != null;
        }
    }
}
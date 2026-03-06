using DTOs;
using Microsoft.Extensions.Logging;
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
        }

        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/brands/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<BrandDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling BrandService for ID {BrandId}", id);
                throw;
            }
        }
    }
}
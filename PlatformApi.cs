using System.Diagnostics.CodeAnalysis;

namespace MicroServices.API.Clients
{
    public class PlatformApi : IPlatformApi
    {
        private readonly HttpClient _httpClient;

        public PlatformApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<IEnumerable<PlatformDto>>> GetAllAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResult<IEnumerable<PlatformDto>>>("api/platforms");
            return result ?? new ApiResult<IEnumerable<PlatformDto>>
            {
                StatusCode = 500,
                IsSuccess = false,
                Payload = Enumerable.Empty<PlatformDto>(),
                Message = "Failed to fetch platforms."
            };
        }

        public async Task<ApiResult<PlatformDto>> GetByIdAsync(int id)
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResult<PlatformDto>>($"api/platforms/{id}");
            return result ?? new ApiResult<PlatformDto>
            {
                StatusCode = 404,
                IsSuccess = false,
                Payload = null!,
                Message = "Platform not found."
            };
        }

        public async Task<ApiResult<IEnumerable<PlatformDto>>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var response = await _httpClient.PostAsJsonAsync("api/platforms/by-ids", ids);
            var result = await response.Content.ReadFromJsonAsync<ApiResult<IEnumerable<PlatformDto>>>();
            return result ?? new ApiResult<IEnumerable<PlatformDto>>
            {
                StatusCode = 500,
                IsSuccess = false,
                Payload = Enumerable.Empty<PlatformDto>(),
                Message = "Failed to fetch platforms by IDs."
            };
        }
    }
}

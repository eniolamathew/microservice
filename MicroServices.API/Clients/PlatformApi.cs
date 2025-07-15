using MicroServices.API.Common;
using MicroServices.API.Interfaces;
using MicroServices.API.Models.Entities;

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
            var result = await _httpClient.GetFromJsonAsync<ApiResult<IEnumerable<PlatformDto>>>("api/platform");
            return result ?? new ApiResult<IEnumerable<PlatformDto>>(
                statusCode: 500,
                isSuccess: false,
                payload: Enumerable.Empty<PlatformDto>(),
                message: "Failed to fetch platforms."
            );
        }

        public async Task<ApiResult<PlatformDto>> GetByIdAsync(int id)
        {
           var result = await _httpClient.GetFromJsonAsync<ApiResult<PlatformDto>>($"api/platform/{id}");
            return result ?? new ApiResult<PlatformDto>(
                statusCode: 404,
                isSuccess: false,
                payload: null!,
                message: "Platform not found."
            );
        }

        public async Task<ApiResult<IEnumerable<PlatformDto>>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var response = await _httpClient.PostAsJsonAsync("api/platform/getbyids", ids);
            var result = await response.Content.ReadFromJsonAsync<ApiResult<IEnumerable<PlatformDto>>>();
            return result ?? new ApiResult<IEnumerable<PlatformDto>>(
                statusCode: 500,
                isSuccess: false,
                payload: Enumerable.Empty<PlatformDto>(),
                message: "Failed to fetch platforms by IDs."
            );
        }
    }
}
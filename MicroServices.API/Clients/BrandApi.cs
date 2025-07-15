using MicroServices.API.Common;
using MicroServices.API.Interfaces;
using MicroServices.API.Models.Entities;

namespace MicroServices.API.Clients
{
    public class BrandApi : IBrandApi
    {
        private readonly HttpClient _httpClient;

        public BrandApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<IEnumerable<BrandDto>>> GetAllAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResult<IEnumerable<BrandDto>>>("api/brand");
            return result ?? new ApiResult<IEnumerable<BrandDto>>(
                statusCode: 500,
                isSuccess: false,
                payload: Enumerable.Empty<BrandDto>(),
                message: "Failed to fetch brands."
            );
        }

        public async Task<ApiResult<BrandDto>> GetByIdAsync(int id)
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResult<BrandDto>>($"api/brand/{id}");
            return result ?? new ApiResult<BrandDto>(
                statusCode: 404,
                isSuccess: false,
                payload: null!,
                message: "Platform not found."
            );
        }

        public async Task<ApiResult<IEnumerable<BrandDto>>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var response = await _httpClient.PostAsJsonAsync("api/platform/getbyids", ids);
            var result = await response.Content.ReadFromJsonAsync<ApiResult<IEnumerable<BrandDto>>>();
            return result ?? new ApiResult<IEnumerable<BrandDto>>(
                statusCode: 500,
                isSuccess: false,
                payload: Enumerable.Empty<BrandDto>(),
                message: "Failed to fetch brands by IDs."
            );
        }
    }
}
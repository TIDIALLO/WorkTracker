using System.Net.Http.Json;
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services
{
    public class AffectationsService : IAffectationsService
    {
        private readonly HttpClient _http;

        public AffectationsService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<AffectationModuleDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<AffectationModuleDto>>("api/affectations") ?? new();
        }

        public async Task<AffectationModuleDto?> GetByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<AffectationModuleDto>($"api/affectations/{id}");
        }

        public async Task<bool> CreateAsync(AffectationModuleDto affectation)
        {
            var response = await _http.PostAsJsonAsync("api/affectations", affectation);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, AffectationModuleDto affectation)
        {
            var response = await _http.PutAsJsonAsync($"api/affectations/{id}", affectation);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/affectations/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

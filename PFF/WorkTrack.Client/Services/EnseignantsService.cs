using System.Net.Http.Json;
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services
{
    public class EnseignantsService : IEnseignantsService
    {
        private readonly HttpClient _httpClient;

        public EnseignantsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EnseignantDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<EnseignantDto>>("api/enseignants") ?? new List<EnseignantDto>();
        }

        public async Task<EnseignantDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<EnseignantDto>($"api/enseignants/{id}");
        }

        public async Task<bool> CreateAsync(EnseignantCreateOrUpdateDto enseignant)
        {
            var response = await _httpClient.PostAsJsonAsync("api/enseignants", enseignant);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, EnseignantCreateOrUpdateDto enseignant)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/enseignants/{id}", enseignant);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/enseignants/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
using System.Net.Http.Json;
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services
{
    public class ModulesService : IModulesService
    {
        private readonly HttpClient _http;

        public ModulesService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ModuleDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<ModuleDto>>("api/modules") ?? new();
        }

        public async Task<ModuleDto?> GetByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<ModuleDto>($"api/modules/{id}");
        }

        public async Task<bool> CreateAsync(ModuleDto module)
        {
            var response = await _http.PostAsJsonAsync("api/modules", module);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, ModuleDto module)
        {
            var response = await _http.PutAsJsonAsync($"api/modules/{id}", module);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/modules/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

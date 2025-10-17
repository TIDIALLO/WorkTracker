using WorkTrack.Client.Services.Interfaces;
using WorkTrack.Client.Models;
using System.Net.Http.Json;


namespace WorkTrack.Client.Services;

public class FilieresService : IFilieresService

{
    private readonly HttpClient _http;

    public FilieresService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<FiliereDto>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<FiliereDto>>("api/filieres") ?? new();
    }

    public async Task<FiliereDto?> GetByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<FiliereDto>($"api/filieres/{id}");
    }

    public async Task<bool> CreateAsync(FiliereDto filiere)
    {
        if (filiere.Id == Guid.Empty)
            filiere.Id = Guid.NewGuid();

        var response = await _http.PostAsJsonAsync("api/filieres", filiere);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(Guid id, FiliereDto filiere)
    {
        var response = await _http.PutAsJsonAsync($"api/filieres/{id}", filiere);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/filieres/{id}");
        return response.IsSuccessStatusCode;
    }
}

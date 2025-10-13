using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace WorkTrack.Client.Services;

public class AuthService(HttpClient http,
                         ILocalStorageService storage,
                         AuthenticationStateProvider authProvider) : IAuthService
{
    private const string TokenKey = "wt.jwt";

    public async Task<bool> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("api/auth/login", new { email, password }, ct);
        if (!resp.IsSuccessStatusCode) return false;

        var json = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (json is null || string.IsNullOrWhiteSpace(json.token)) return false;

        await storage.SetItemAsStringAsync(TokenKey, json.token, ct);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.token);

        // informer le provider
        if (authProvider is JwtAuthenticationStateProvider jwt)
            await jwt.SetTokenAsync(json.token);

        return true;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        await storage.RemoveItemAsync(TokenKey, ct);
        http.DefaultRequestHeaders.Authorization = null;

        if (authProvider is JwtAuthenticationStateProvider jwt)
            await jwt.ClearAsync();
    }

    private sealed record LoginResponse(string token);
}

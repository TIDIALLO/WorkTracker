using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace WorkTrack.Client.Services;

public class JwtAuthenticationStateProvider(ILocalStorageService storage) : AuthenticationStateProvider
{
    private const string TokenKey = "wt.jwt";
    private ClaimsPrincipal _user = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_user));

    public async Task InitializeAsync()
    {
        var token = await storage.GetItemAsStringAsync(TokenKey);
        if (!string.IsNullOrWhiteSpace(token))
            _user = BuildPrincipalFromToken(token);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SetTokenAsync(string token)
    {
        await storage.SetItemAsStringAsync(TokenKey, token);
        _user = BuildPrincipalFromToken(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task ClearAsync()
    {
        await storage.RemoveItemAsync(TokenKey);
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static ClaimsPrincipal BuildPrincipalFromToken(string jwt)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var identity = new ClaimsIdentity(token.Claims, authenticationType: "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}

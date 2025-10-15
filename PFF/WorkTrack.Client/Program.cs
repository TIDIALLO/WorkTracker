using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WorkTrack.Client;
using WorkTrack.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Base API depuis appsettings.json, avec fallback
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5023";

// Libs d’auth côté client
builder.Services.AddBlazoredLocalStorage();     // stockage du JWT
builder.Services.AddAuthorizationCore();        // AuthorizeView, etc.

// HttpClient pour appeler l’API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });

// Services applicatifs
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISeancesService, SeancesService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IEnseignantsService, EnseignantsService>();


var host = builder.Build();

// 🔁 Au démarrage : remettre le JWT si déjà présent
var storage = host.Services.GetRequiredService<ILocalStorageService>();
var http = host.Services.GetRequiredService<HttpClient>();
var token = await storage.GetItemAsStringAsync("wt.jwt");
if (!string.IsNullOrWhiteSpace(token))
{
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    // notifier le provider pour hydrater l’état d’auth
    var authProvider = (JwtAuthenticationStateProvider)host.Services.GetRequiredService<AuthenticationStateProvider>();
    await authProvider.SetTokenAsync(token);
}

await host.RunAsync();

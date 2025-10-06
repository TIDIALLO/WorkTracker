using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WorkTrack.Client;
using WorkTrack.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Base API depuis appsettings.json, avec fallback
var apiBase = builder.Configuration["ApiBaseUrl"] 
              ?? "https://localhost:5023";

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBase)
});

// Services applicatifs (typed)
builder.Services.AddScoped<ISeancesService, SeancesService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

await builder.Build().RunAsync();

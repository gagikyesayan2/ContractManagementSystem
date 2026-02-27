using Blazored.LocalStorage;
using ClientSide;
using ClientSide.Http;
using ClientSide.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<CompanyApiService>();

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

// handler
builder.Services.AddScoped<AuthMessageHandler>();

// IMPORTANT: use HttpClientFactory so handler is used
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7290/"); // your API base
})
.AddHttpMessageHandler<AuthMessageHandler>();

// Provide HttpClient for injections
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

await builder.Build().RunAsync();







// 1) Register our custom DelegatingHandler in DI (Dependency Injection)
//
// AuthMessageHandler is the piece that:
// - reads "access_token" from localStorage
// - adds "Authorization: Bearer <token>" header to outgoing HTTP requests
//
// Why Scoped?
// - In Blazor WebAssembly, "Scoped" behaves like "per user / per app instance" (similar to Singleton),
//   but using Scoped is the standard pattern for Http handlers + services.
// - It allows the handler to resolve other scoped services (like ILocalStorageService) safely.
//builder.Services.AddScoped<AuthMessageHandler>();


// 2) Register a named HttpClient using IHttpClientFactory
//
// Why do we use HttpClientFactory (AddHttpClient) instead of "new HttpClient()"?
// - Because HttpClientFactory allows us to compose a pipeline (handlers / policies / logging)
// - And MOST IMPORTANT: it guarantees our AuthMessageHandler is actually used.
//
// "ApiClient" is just a name (string key). We'll later request this exact client by name.
//builder.Services.AddHttpClient("ApiClient", client =>
//{
//    // BaseAddress means:
//    // - every request like "api/auth/signin" becomes:
//      https://localhost:7290/api/auth/signin
//    
// IMPORTANT:
// - This must point to your BACKEND API host/port, not the Blazor client port.
// - In production, this will be your real domain: https://api.yourdomain.com/
//    client.BaseAddress = new Uri("https://localhost:7290/"); // backend API base URL
//})


// 3) Attach our AuthMessageHandler to this HttpClient
//
// This creates an outgoing request pipeline:
// HttpClient -> AuthMessageHandler -> (real network call)
//
// Meaning:
// - Every request created by this named client automatically passes through AuthMessageHandler.
// - So after the user signs in and tokens are stored, every subsequent request automatically
//   includes: Authorization: Bearer <access_token>
//
// Without this line:
// - You would store tokens, but API requests would still be unauthorized (401)
//   because no header is added.
//.AddHttpMessageHandler<AuthMessageHandler>();


// 4) Provide HttpClient for normal constructor injection (DI)
//
// Many services (like AuthApiClient, CompanyApiClient, ContractApiClient) are written like:
//   public ContractApiClient(HttpClient http) { ... }
//
// If we do nothing, injecting HttpClient might give you a "default" one without our handler.
// So we explicitly tell DI:
//
// - Whenever someone asks for HttpClient,
// - give them the named client "ApiClient" (the one with BaseAddress + AuthMessageHandler)
//
// IHttpClientFactory.CreateClient("ApiClient") returns the configured HttpClient instance.
//
// Why Scoped?
// - Same reasoning: in WASM, Scoped is the correct default lifetime.
// - Also, HttpClient is intended to be reused, not created per request manually.
//builder.Services.AddScoped(sp =>
//    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));


// 5) Build the app and run it
//
// - Build() creates the final service provider + app pipeline.
// - RunAsync() starts the Blazor WebAssembly runtime.
//
// After this runs:
// - Your pages load
// - Your services can inject HttpClient
// - And every API call automatically includes the Bearer token (if it exists in localStorage).
//await builder.Build().RunAsync();
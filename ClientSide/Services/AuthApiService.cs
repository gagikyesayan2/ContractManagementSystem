using System.Net.Http.Json;
using Blazored.LocalStorage;
using ContractManagementSystem.Shared.Models.Account;



namespace ClientSide.Services;

public sealed class AuthApiClient
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthStateService _authState;
    private readonly JwtAuthStateProvider _authProvider;

    public AuthApiClient(HttpClient http, ILocalStorageService localStorage, AuthStateService authState, JwtAuthStateProvider authProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authState = authState;
        _authProvider = authProvider;
    }

    public async Task<RefreshTokenResponseModel> SignInAsync(SignInRequestModel req, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("api/auth/signin", req, ct);

        var body = await res.Content.ReadFromJsonAsync<RefreshTokenResponseModel>(cancellationToken: ct);

        if (!res.IsSuccessStatusCode || body is null)
            throw new Exception("Sign-in failed.");

        await SaveTokensAsync(body, ct);


        // make header update immediately
        await _authState.RefreshAsync(ct);

        return body;
    }

    public async Task<int> SignUpAsync(SignUpRequestModel req, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("api/auth/signup", req, ct);

        if (!res.IsSuccessStatusCode)
            throw new Exception("Sign-up failed.");

        var result = await res.Content.ReadFromJsonAsync<int>(cancellationToken: ct);

        return result;
    }

    public async Task<RefreshTokenResponseModel> RefreshAsync(RefreshTokenRequestModel req, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("api/auth/refresh", req, ct);

        var body = await res.Content.ReadFromJsonAsync<RefreshTokenResponseModel>(cancellationToken: ct);

        if (!res.IsSuccessStatusCode || body is null)
            throw new Exception("Refresh failed.");

        await SaveTokensAsync(body, ct);
        return body;
    }

    public async Task SaveTokensAsync(RefreshTokenResponseModel tokens, CancellationToken ct)
    {
        await _localStorage.SetItemAsync("access_token", tokens.AccessToken, ct);
        await _localStorage.SetItemAsync("refresh_token", tokens.RefreshToken, ct);

        _authProvider.NotifyAuthStateChanged();
    }
}
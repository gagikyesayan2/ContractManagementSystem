using Blazored.LocalStorage;

namespace ClientSide.Services;

public sealed class AuthStateService
{
    private readonly ILocalStorageService _localStorage;

    public AuthStateService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public bool IsAuthenticated { get; private set; }

    public event Action? OnChange;

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        var token = await _localStorage.GetItemAsync<string>("access_token", ct);
        var newValue = !string.IsNullOrWhiteSpace(token);

        if (newValue != IsAuthenticated)
        {
            IsAuthenticated = newValue;
            OnChange?.Invoke();
        }
    }
}
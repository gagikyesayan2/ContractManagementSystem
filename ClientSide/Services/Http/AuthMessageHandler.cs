using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace ClientSide.Http;

public sealed class AuthMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
    {

        // Don’t attach token if request already has Authorization header
        if (request.Headers.Authorization is null)
        {
            var token = await _localStorage.GetItemAsync<string>("access_token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
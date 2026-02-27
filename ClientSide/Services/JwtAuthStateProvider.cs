using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ClientSide.Services
{
   public sealed class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        public JwtAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("access_token");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), authenticationType: "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        public void NotifyAuthStateChanged()
            => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            // IMPORTANT:
            // Roles might be in "role" or "roles" or a full URI claim type depending on your backend.
            // We return all claims and let Blazor role checks work if the claim type matches ClaimTypes.Role or "role".
            var claims = token.Claims.ToList();

            // If backend uses "role" (common), map it to ClaimTypes.Role so [Authorize(Roles="Admin")] works.
            var roleClaims = claims.Where(c => c.Type is "role" or "roles").ToList();
            foreach (var rc in roleClaims)
                claims.Add(new Claim(ClaimTypes.Role, rc.Value));

            return claims;
        }
    }
    }

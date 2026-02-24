using System.Security.Claims;

namespace ContractManagementSystem.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetAccountId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(value))
                throw new UnauthorizedAccessException("AccountId claim missing.");

            return Guid.Parse(value);
        }
    }
}

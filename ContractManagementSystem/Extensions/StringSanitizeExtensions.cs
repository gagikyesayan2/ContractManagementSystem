namespace ContractManagementSystem.API.Extensions;

public static class StringSanitizeExtensions
{
    public static string? Sanitize(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

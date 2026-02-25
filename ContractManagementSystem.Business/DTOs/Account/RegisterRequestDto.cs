using ContractManagementSystem.Data.Enums;

namespace ContractManagementSystem.Business.DTOs.Account;

public sealed class SignUpRequestDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

namespace ContractManagementSystem.Business.DTOs.Account;

public sealed class SignInRequestDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

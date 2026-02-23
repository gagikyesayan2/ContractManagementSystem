namespace ContractManagementSystem.Business.DTOs.Account;

public sealed class SignInResponseDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;

}

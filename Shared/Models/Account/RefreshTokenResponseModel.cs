namespace ContractManagementSystem.Shared.Models.Account;

public class RefreshTokenResponseModel
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

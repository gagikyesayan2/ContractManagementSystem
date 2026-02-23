namespace ContractManagementSystem.API.Models.Account;

public class RefreshResponseModel
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

using System.ComponentModel.DataAnnotations;
namespace ContractManagementSystem.Shared.Models.Account;

public class RefreshTokenRequestModel
{
    [Required]
    public string RefreshToken { get; set; } = default!;

}

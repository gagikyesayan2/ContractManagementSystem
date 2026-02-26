using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.API.Models.Account;

public class RefreshTokenRequestModel
{
    [Required]
    public string RefreshToken { get; set; } = default!;

}

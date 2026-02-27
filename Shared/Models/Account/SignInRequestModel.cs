using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.Shared.Models.Account;

public sealed class SignInRequestModel
{

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;
    [Required]
    public string Password { get; set; } = default!;
}

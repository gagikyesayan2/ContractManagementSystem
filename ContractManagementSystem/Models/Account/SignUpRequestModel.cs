using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.API.Models.Account;

public sealed class SignUpRequestModel
{

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}


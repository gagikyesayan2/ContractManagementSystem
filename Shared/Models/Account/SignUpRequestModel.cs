using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.Shared.Models.Account;

public sealed class SignUpRequestModel
{

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}


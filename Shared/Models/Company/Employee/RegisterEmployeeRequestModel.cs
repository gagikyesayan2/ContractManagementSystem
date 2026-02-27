using System.ComponentModel.DataAnnotations;
namespace ContractManagementSystem.Shared.Models.Employee;


public sealed class RegisterEmployeeRequestModel
{
    [Required]
    [EmailAddress]

    public string Email { get; set; } = default!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }   
}

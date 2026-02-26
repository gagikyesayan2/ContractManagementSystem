using System.ComponentModel.DataAnnotations;

namespace ContractManagementSystem.API.Models.Company.Employee;

public sealed class RegisterEmployeeRequestModel
{
    [Required]
    [EmailAddress]

    public string Email { get; set; } = default!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }   
}

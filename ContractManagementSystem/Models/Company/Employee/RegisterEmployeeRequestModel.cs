namespace ContractManagementSystem.API.Models.Company.Employee;

public sealed class RegisterEmployeeRequestModel
{
    public string Email { get; set; } = default!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }   
}

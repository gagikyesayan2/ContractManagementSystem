namespace ContractManagementSystem.Business.DTOs.Company.Employee;

public sealed class RegisterEmployeeRequestDto
{
    public Guid CompanyId { get; set; }
    public string Email { get; set; } = default!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}

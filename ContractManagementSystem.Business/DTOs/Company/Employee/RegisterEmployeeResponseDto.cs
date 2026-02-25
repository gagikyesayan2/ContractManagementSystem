namespace ContractManagementSystem.Business.DTOs.Company.Employee;

public sealed class RegisterEmployeeResponseDto
{
    public Guid EmployeeAccountId { get; set; }
    public string Email { get; set; } = default!;
    public string TemporaryPassword { get; set; } = default!;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

namespace ContractManagementSystem.Business.DTOs.Company;

public sealed class RegisterEmployeeRequestDto
{
    public Guid CompanyId { get; set; }
    public string Email { get; set; } = default!;
}

namespace ContractManagementSystem.API.Models.Company;

public sealed class RegisterEmployeeResponseModel
{
    public Guid EmployeeAccountId { get; set; }
    public string Email { get; set; } = default!;
    public string TemporaryPassword { get; set; } = default!;
}

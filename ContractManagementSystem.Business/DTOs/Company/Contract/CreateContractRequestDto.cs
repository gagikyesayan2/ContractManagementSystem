namespace ContractManagementSystem.Business.DTOs.Company.Contract;

public class CreateContractRequestDto
{
    public Guid CompanyId { get; init; }
    public Guid EmployeeAccountId { get; init; }

    public string Title { get; init; } = default!;
    public string? Description { get; init; }

    public DateOnly EmploymentStartDate { get; init; }
    public DateOnly? EmploymentEndDate { get; init; }

    public decimal Wage { get; init; }
}

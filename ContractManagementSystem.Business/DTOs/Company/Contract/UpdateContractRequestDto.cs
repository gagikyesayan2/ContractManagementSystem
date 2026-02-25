namespace ContractManagementSystem.Business.DTOs.Company.Contract;

public sealed class UpdateContractRequestDto
{
    public Guid Id { get; set; }
    public string Title { get; init; } = default!;
    public string? Description { get; init; }

    public DateOnly EmploymentStartDate { get; init; }
    public DateOnly? EmploymentEndDate { get; init; }

    public decimal Wage { get; init; }
}
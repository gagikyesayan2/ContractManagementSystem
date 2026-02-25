namespace ContractManagementSystem.Business.DTOs.Company.Contract;

public sealed class ContractListItemDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public Guid EmployeeAccountId { get; init; }

    public string Title { get; init; } = default!;
    public string? Description { get; init; }

    public DateOnly EmploymentStartDate { get; init; }
    public DateOnly? EmploymentEndDate { get; init; }

    public decimal Wage { get; init; }
    public DateTime CreatedAtUtc { get; init; }

    // Optional if available in DB join:
    public string? EmployeeFirstName { get; init; }
    public string? EmployeeLastName { get; init; }
}

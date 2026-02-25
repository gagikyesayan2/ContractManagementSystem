namespace ContractManagementSystem.API.Models.Company.Contract;

public sealed class UpdateContractResponseModel
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
    public DateTime? UpdatedAtUtc { get; init; }   // frontend usually needs this

    // Employee display info (for dropdowns / UI labels)
    public string? EmployeeFirstName { get; init; }
    public string? EmployeeLastName { get; init; }
}

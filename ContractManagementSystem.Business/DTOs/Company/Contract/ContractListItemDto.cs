namespace ContractManagementSystem.Business.DTOs.Company.Contract;

public sealed class ContractListItemDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public DateOnly EmploymentStartDate { get; set; }
    public DateOnly? EmploymentEndDate { get; set; }

    public decimal Wage { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    // Optional if available in DB join:
    public string? EmployeeFirstName { get; set; }
    public string? EmployeeLastName { get; set; }
}

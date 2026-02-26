using ContractManagementSystem.Data.Entities.Common;

namespace ContractManagementSystem.Data.Entities;

public sealed class Contract : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid EmployeeAccountId { get; set; }

    public string Title { get; set; } = default!;
    public string ?Description { get; set; } = default!;

    public DateOnly EmploymentStartDate { get; set; }
    public DateOnly? EmploymentEndDate { get; set; }

    public decimal Wage { get; set; }

    public string? EmployeeFirstName { get; set; } = default!; // for joins (no real field)
    public string? EmployeeLastName { get; set; } = default!; // for joins (no real field)
}

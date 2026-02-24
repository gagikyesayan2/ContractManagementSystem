using ContractManagementSystem.Data.Entities.Common;

namespace ContractManagementSystem.Data.Entities;

public sealed class Contract : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid EmployeeAccountId { get; set; }

    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;

    public DateTime EmploymentStartDate { get; set; }
    public DateTime EmploymentEndDate { get; set; }

    public decimal Wage { get; set; }
}

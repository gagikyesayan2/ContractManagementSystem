using ContractManagementSystem.Data.Entities.Common;

namespace ContractManagementSystem.Data.Entities;

public sealed class Contract : BaseEntity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime EmploymentStartDate { get; set; }
    public DateTime EmploymentEndDate { get; set; }
    public Decimal Wage { get; set; }
    public bool IsExpired => DateTime.UtcNow > EmploymentEndDate;
}

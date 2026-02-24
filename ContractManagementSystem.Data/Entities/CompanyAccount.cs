namespace ContractManagementSystem.Data.Entities;

public sealed class CompanyAccount
{
    public Guid CompanyId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime JoinedAtUtc { get; set; }
}

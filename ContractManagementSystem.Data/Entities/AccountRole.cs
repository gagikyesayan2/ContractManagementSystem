
namespace ContractManagementSystem.Data.Entities;

public sealed class AccountRole
{
    public Guid AccountId { get; set; }
    public int RoleId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
namespace ContractManagementSystem.Business.DTOs.Account;

public sealed class AccountRoleCreateDto
{
    public Guid AccountId { get; set; }
    public int RoleId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

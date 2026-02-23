using ContractManagementSystem.Data.Entities.Common;

namespace ContractManagementSystem.Data.Entities;

public sealed class RefreshToken : BaseEntity
{

    public Guid AccountId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked { get; private set; }
    public void Revoke()
    {
        IsRevoked = true;
        RevokedAtUtc = DateTime.UtcNow;
    }
}

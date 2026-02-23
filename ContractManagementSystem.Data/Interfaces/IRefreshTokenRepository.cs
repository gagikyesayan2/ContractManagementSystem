using ContractManagementSystem.Data.Entities;

namespace ContractManagementSystem.Data.Interfaces;

public interface IRefreshTokenRepository
{
    Task<int> InsertAsync(RefreshToken row);
    Task<RefreshToken?> GetByTokenHashId(string tokenHash);

    Task<RefreshToken> GetActiveByAccountIdAsync(Guid accountId);
    Task<int> RevokeAsync(Guid tokenId, DateTime revokedAtUtc);
    Task<int> SaveAsync(RefreshToken token);
}
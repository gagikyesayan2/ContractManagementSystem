using ContractManagementSystem.Data.Entities;

namespace ContractManagementSystem.Data.Interfaces;

public interface IContractRepository
{
    Task<Guid> AddAsync(Contract contract, Guid createdByAdminAccountId, CancellationToken ct = default);
}

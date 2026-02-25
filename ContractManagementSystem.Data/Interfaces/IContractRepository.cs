using ContractManagementSystem.Data.Entities;

namespace ContractManagementSystem.Data.Interfaces;

public interface IContractRepository
{
    Task<bool> DeleteAsync(Guid contractId, Guid companyId, CancellationToken ct);

    Task<Contract> AddAsync(Contract contract, Guid createdByAdminAccountId, CancellationToken ct = default);

    Task<Contract> UpdateAsync(Contract contract,Guid editedByAdminAccountId,CancellationToken ct);
}

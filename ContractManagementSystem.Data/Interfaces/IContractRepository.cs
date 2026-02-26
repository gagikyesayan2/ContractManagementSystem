using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Enums;

namespace ContractManagementSystem.Data.Interfaces;

public interface IContractRepository
{
    Task<bool> DeleteAsync(Guid contractId, Guid companyId, CancellationToken ct);

    Task<Contract> AddAsync(Contract contract, Guid createdByAdminAccountId, CancellationToken ct = default);

    Task<Contract> UpdateAsync(Contract contract,Guid editedByAdminAccountId,CancellationToken ct);

    Task<IReadOnlyList<Contract>> GetAllByCompanyIdAsync(Guid companyId, CancellationToken ct = default);

    Task<IReadOnlyList<Contract>> GetByStatusAsync(Guid companyId, ContractStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<Contract>> SearchAsync(Contract contract, CancellationToken ct = default);

}

using ContractManagementSystem.Business.DTOs.Company.Contract;
using ContractManagementSystem.Business.Enum;

namespace ContractManagementSystem.Business.Interfaces;

public interface IContractService
{
    Task<ContractListItemDto> CreateContractAsync(Guid createdByAdminAccountId, CreateContractRequestDto dto, CancellationToken ct);
    Task<bool> DeleteContractAsync(DeleteContractRequestDto requestDto, CancellationToken ct);
    Task<ContractListItemDto> EditContractAsync(Guid editedByAdminAccountId, UpdateContractRequestDto dto, CancellationToken ct);

    //Task DeleteContractAsync(Guid deletedByAdminAccountId, Guid contractId);

    //Task<IReadOnlyList<ContractListItemDto>> SearchContractsAsync(Guid requesterAccountId,Guid companyId,string? title,string? employeeFirstName,string? employeeLastName);

    //Task<IReadOnlyList<ContractListItemDto>> GetCompanyContractsAsync(Guid requesterAccountId,Guid companyId, ContractStatusFilter status);
}

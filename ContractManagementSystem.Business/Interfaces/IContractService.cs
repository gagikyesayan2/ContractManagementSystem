using ContractManagementSystem.Business.DTOs.Company.Contract;
using ContractManagementSystem.Data.Enums;

namespace ContractManagementSystem.Business.Interfaces;

public interface IContractService
{
    Task<ContractListItemDto> CreateContractAsync(Guid createdByAdminAccountId, CreateContractRequestDto dto, CancellationToken ct);
    Task<bool> DeleteContractAsync(DeleteContractRequestDto requestDto, CancellationToken ct);
    Task<ContractListItemDto> EditContractAsync(Guid editedByAdminAccountId, UpdateContractRequestDto dto, CancellationToken ct);

    Task<IReadOnlyList<ContractListItemDto>> GetAllContractsAsync(Guid requesterAccountId, Guid companyId);

    Task<IReadOnlyList<ContractListItemDto>> GetContractsByStatusAsync(Guid requesterAccountId, Guid companyId, string status);

    Task<IReadOnlyList<ContractListItemDto>> SearchContractsAsync(ContractListItemDto requestDto, CancellationToken ct);


    
}

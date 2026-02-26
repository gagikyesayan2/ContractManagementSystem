using AutoMapper;
using ContractManagementSystem.Business.DTOs.Company.Contract;
using ContractManagementSystem.Business.Exceptions.Common;
using ContractManagementSystem.Business.Interfaces;
using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Enums;
using ContractManagementSystem.Data.Interfaces;
using System.ComponentModel.Design;

namespace ContractManagementSystem.Application.Services;

public sealed class ContractService(ICompanyRepository companyRepository, IContractRepository contractRepository, IMapper mapper) : IContractService
{



    public async Task<ContractListItemDto> CreateContractAsync(Guid createdByAdminAccountId, CreateContractRequestDto requestDto, CancellationToken ct)
    {
        // Business validation (DB will also validate admin/company/employee membership)
        if (string.IsNullOrWhiteSpace(requestDto.Title))
            throw new ValidationAppException("Title is required.");

        if (requestDto.Wage <= 0)
            throw new ValidationAppException("Wage must be greater than 0.");

        if (requestDto.EmploymentEndDate is not null && requestDto.EmploymentEndDate < requestDto.EmploymentStartDate)
            throw new ValidationAppException("EmploymentEndDate cannot be earlier than EmploymentStartDate.");

        var requestEntity = mapper.Map<Contract>(requestDto);

        var responseEntity = await contractRepository.AddAsync(requestEntity, createdByAdminAccountId, ct);
        var responseDto = mapper.Map<ContractListItemDto>(responseEntity);

        return responseDto;
    }

    public async Task<ContractListItemDto> EditContractAsync(Guid editedByAdminAccountId, UpdateContractRequestDto requestDto, CancellationToken ct)
    {
        if (requestDto.Id == Guid.Empty)
            throw new ValidationAppException("Invalid contractId.");

        if (requestDto.EmploymentEndDate is not null && requestDto.EmploymentEndDate < requestDto.EmploymentStartDate)
            throw new ValidationAppException("EmploymentEndDate cannot be earlier than EmploymentStartDate.");

        // We pass the full entity state to repo update.
        // Repo should enforce: (1) contract exists, (2) editor is admin for contract’s company, etc.

        var requestEntity = mapper.Map<Contract>(requestDto);
        var responseEntity = await contractRepository.UpdateAsync(requestEntity, editedByAdminAccountId, ct);

        var responseDto = mapper.Map<ContractListItemDto>(responseEntity);

        return responseDto;

    }

    public async Task<bool> DeleteContractAsync(DeleteContractRequestDto dto, CancellationToken ct)
    {
        var isAdmin = await companyRepository.IsAdminAsync(dto.CompanyId, dto.DeletedByAdminAccountId, ct);

        if (!isAdmin)
            throw new UnauthorizedAppException("User is not admin of this company.");

        var deleted = await contractRepository.DeleteAsync(dto.ContractId, dto.CompanyId, ct);

        if (!deleted)
            throw new NotFoundAppException("Contract not found.");

        return true;
    }

    public async Task<IReadOnlyList<ContractListItemDto>> GetAllContractsAsync(Guid requesterAccountId, Guid companyId)
    {
        // Security: user must be a member of the company to view its contracts
        var isMember = await companyRepository.IsValidEmployeeInCompany(companyId, requesterAccountId);
        if (!isMember)
            throw new UnauthorizedAppException("You are not a member of this company.");

        var resultEntity = await contractRepository.GetAllByCompanyIdAsync(companyId);

        var resultDto = mapper.Map<IReadOnlyList<ContractListItemDto>>(resultEntity);

        return resultDto;
    }

    public async Task<IReadOnlyList<ContractListItemDto>> GetContractsByStatusAsync(Guid requesterAccountId, Guid companyId, string status)
    {
        var isMember = await companyRepository.IsValidEmployeeInCompany(companyId, requesterAccountId);

        if (!isMember)
            throw new UnauthorizedAppException("You are not a member of this company.");

        if (!Enum.TryParse<ContractStatus>(status, true, out var parsedStatus))
            throw new ValidationAppException(
                "Invalid status. Use: NotStarted, Active, Finished.");

        var entities = await contractRepository.GetByStatusAsync(companyId, parsedStatus);

        var resultDto = mapper.Map<IReadOnlyList<ContractListItemDto>>(entities);

        return resultDto;
    }



    public async Task<IReadOnlyList<ContractListItemDto>> SearchContractsAsync(ContractListItemDto requestDto, CancellationToken ct)
    {
        var requestEntity = mapper.Map<Contract>(requestDto);

        var resultEntity = await contractRepository.SearchAsync(requestEntity, ct);

        var resultDto = mapper.Map<IReadOnlyList<ContractListItemDto>>(resultEntity);

        return resultDto;
    }

}


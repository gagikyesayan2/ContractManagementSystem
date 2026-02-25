using AutoMapper;
using ContractManagementSystem.Business.DTOs.Company.Contract;
using ContractManagementSystem.Business.Interfaces;
using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Repositories;

namespace ContractManagementSystem.Application.Services;

public sealed class ContractService(ICompanyRepository companyRepository, IContractRepository contractRepository, IMapper mapper) : IContractService
{



    public async Task<ContractListItemDto> CreateContractAsync(Guid createdByAdminAccountId, CreateContractRequestDto requestDto, CancellationToken ct)
    {
        // Business validation (DB will also validate admin/company/employee membership)
        if (string.IsNullOrWhiteSpace(requestDto.Title))
            throw new ArgumentException("Title is required.", nameof(requestDto.Title));

        if (requestDto.Wage <= 0)
            throw new ArgumentException("Wage must be greater than 0.", nameof(requestDto.Wage));

        if (requestDto.EmploymentEndDate is not null && requestDto.EmploymentEndDate < requestDto.EmploymentStartDate)
            throw new ArgumentException("EmploymentEndDate cannot be earlier than EmploymentStartDate.");

        var requestEntity = mapper.Map<Contract>(requestDto);

        var responseEntity = await contractRepository.AddAsync(requestEntity, createdByAdminAccountId, ct);
        var responseDto = mapper.Map<ContractListItemDto>(responseEntity);
        
        return responseDto;
    }

    public async Task<ContractListItemDto> EditContractAsync(Guid editedByAdminAccountId, UpdateContractRequestDto requestDto, CancellationToken ct)
    {
        if (requestDto.Id == Guid.Empty)
            throw new ArgumentException("Invalid contractId.");

        if (string.IsNullOrWhiteSpace(requestDto.Title))
            throw new ArgumentException("Title is required.", nameof(requestDto.Title));

        if (requestDto.Wage <= 0)
            throw new ArgumentException("Wage must be greater than 0.", nameof(requestDto.Wage));

        if (requestDto.EmploymentEndDate is not null && requestDto.EmploymentEndDate < requestDto.EmploymentStartDate)
            throw new ArgumentException("EmploymentEndDate cannot be earlier than EmploymentStartDate.");

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
            throw new UnauthorizedAccessException("User is not admin of this company.");

        var deleted = await contractRepository.DeleteAsync(dto.ContractId, dto.CompanyId, ct);

        if (!deleted)
            throw new InvalidOperationException("Contract not found.");

        return true;
    }

    //public Task<IReadOnlyList<ContractListItemDto>> SearchContractsAsync(
    //    Guid requesterAccountId,
    //    Guid companyId,
    //    string? title,
    //    string? employeeFirstName,
    //    string? employeeLastName,
    //    CancellationToken ct)
    //{
    //    // Normalize inputs for consistent searching
    //    title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
    //    employeeFirstName = string.IsNullOrWhiteSpace(employeeFirstName) ? null : employeeFirstName.Trim();
    //    employeeLastName = string.IsNullOrWhiteSpace(employeeLastName) ? null : employeeLastName.Trim();

    //    return _contracts.SearchAsync(requesterAccountId, companyId, title, employeeFirstName, employeeLastName, ct);
    //}

    //public async Task<IReadOnlyList<ContractListItemDto>> GetCompanyContractsAsync(Guid requesterAccountId,
    //    Guid companyId,
    //    ContractStatusFilter status,
    //    CancellationToken ct)
    //{
    //    var items = await _contracts.GetByCompanyAsync(requesterAccountId, companyId, ct);

    //    if (status == ContractStatusFilter.All)
    //        return items;

    //    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    //    // Filter rules:
    //    // NotStarted: StartDate > today
    //    // Active: StartDate <= today && (EndDate is null || EndDate >= today)
    //    // Finished: EndDate < today (EndDate must exist and be before today)
    //    return status switch
    //    {
    //        ContractStatusFilter.NotStarted =>
    //            items.Where(c => c.EmploymentStartDate > today).ToList(),

    //        ContractStatusFilter.Active =>
    //            items.Where(c =>
    //                c.EmploymentStartDate <= today &&
    //                (c.EmploymentEndDate is null || c.EmploymentEndDate >= today)
    //            ).ToList(),

    //        ContractStatusFilter.Finished =>
    //            items.Where(c => c.EmploymentEndDate is not null && c.EmploymentEndDate < today).ToList(),

    //        _ => items
    //    };
}

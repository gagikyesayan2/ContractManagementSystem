using ContractManagementSystem.Business.DTOs.Company;
using ContractManagementSystem.Business.DTOs.Company.Employee;

namespace ContractManagementSystem.Business.Interfaces;

public interface ICompanyService
{
    Task<CreateCompanyResponseDto> CreateCompanyAsync(CreateCompanyRequestDto requestDto, Guid AccountId, CancellationToken ct = default);
    Task<RegisterEmployeeResponseDto> RegisterEmployeeAsync(RegisterEmployeeRequestDto request, Guid adminAccountId, CancellationToken ct = default);
    Task<IReadOnlyList<CreateCompanyResponseDto>> GetMyAdminCompaniesAsync(Guid adminAccountId, CancellationToken ct = default);
}
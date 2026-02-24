using ContractManagementSystem.Business.DTOs.Company;

namespace ContractManagementSystem.Business.Interfaces;

public interface ICompanyService
{
    Task<CreateCompanyResponseDto> CreateCompanyAsync(CreateCompanyRequestDto requestDto, Guid AccountId, CancellationToken ct = default);
    Task<RegisterEmployeeResponseDto> RegisterEmployeeAsync(RegisterEmployeeRequestDto request, Guid adminAccountId, CancellationToken ct = default);

}
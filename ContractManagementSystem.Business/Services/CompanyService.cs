using AutoMapper;
using ContractManagementSystem.Business.DTOs.Company;
using ContractManagementSystem.Business.Interfaces;
using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Business.Common.Security;
using ContractManagementSystem.Business.DTOs.Company.Employee;
namespace ContractManagementSystem.Business.Services;

public sealed class CompanyService(ICompanyRepository companyRepository, IMapper mapper) : ICompanyService
{
    public async Task<CreateCompanyResponseDto> CreateCompanyAsync(CreateCompanyRequestDto requestDto, Guid AccountId, CancellationToken ct = default)
    {

        var companyEntity = mapper.Map<Company>(requestDto);


        var resultEntity = await companyRepository.CreateWithAdminAsync(companyEntity, AccountId, ct);


        var resultDto = mapper.Map<CreateCompanyResponseDto>(resultEntity);

        return resultDto;
    }

    
        public async Task<RegisterEmployeeResponseDto> RegisterEmployeeAsync(RegisterEmployeeRequestDto requestDto,Guid adminAccountId,CancellationToken ct = default)
        {
            var tempPassword = PasswordHasher.GenerateTempPassword(12);
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            var employeeAccountId = await companyRepository.AddEmployeeAsync(
                requestDto.CompanyId,
                requestDto.Email,
                passwordHash,
                adminAccountId,
                requestDto.FirstName,
                requestDto.LastName,
                ct);

            return new RegisterEmployeeResponseDto
            {
                EmployeeAccountId = employeeAccountId,
                Email = requestDto.Email,
                TemporaryPassword = tempPassword,
                FirstName = requestDto.FirstName,
                LastName = requestDto.LastName
            };
        }

      
}

using AutoMapper;
using ContractManagementSystem.Business.DTOs.Account;
using ContractManagementSystem.Business.DTOs.Company;
using ContractManagementSystem.Data.Entities;

namespace ContractManagementSystem.Business.Mapping;

public sealed class BusinessMappingProfile : Profile
{
    public BusinessMappingProfile()
    {
        CreateMap<SignUpRequestDto, Account>()
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore());


        CreateMap<CreateCompanyRequestDto, Company>()
            .ForMember(d => d.Id, opt => opt.Ignore())           // repo sets
            .ForMember(d => d.CreatedAtUtc, opt => opt.Ignore()); // repo sets

        CreateMap<Company, CreateCompanyResponseDto>();


    }
}

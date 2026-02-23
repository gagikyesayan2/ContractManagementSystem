using ContractManagementSystem.Data.Entities;
using AutoMapper;
using ContractManagementSystem.Business.DTOs.Account;

namespace ContractManagementSystem.Business.Mapping;

public sealed class BusinessMappingProfile : Profile
{
    public BusinessMappingProfile()
    {
        CreateMap<SignUpRequestDto, Account>()
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore());

     
    }
}

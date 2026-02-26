using AutoMapper;
using ContractManagementSystem.Business.DTOs.Account;
using ContractManagementSystem.Business.DTOs.Company;
using ContractManagementSystem.Business.DTOs.Company.Contract;
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

        CreateMap<Company, CreateCompanyResponseDto>()
         .ForMember(dest => dest.CompanyId,
                    opt => opt.MapFrom(src => src.Id));

        // contract mapping 


        CreateMap<CreateContractRequestDto, Contract>()
            .ForMember(d => d.Title,
                o => o.MapFrom(s => s.Title.Trim()))
            
            .ForMember(d => d.Description,
                o => o.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.Description)
                        ? null
                        : s.Description.Trim()));

        CreateMap<UpdateContractRequestDto, Contract>()
        .ForMember(d => d.Title,
            o => o.MapFrom(s => s.Title.Trim()))
        .ForMember(d => d.Description,
            o => o.MapFrom(s =>
                string.IsNullOrWhiteSpace(s.Description)
                    ? null
                    : s.Description.Trim()))

      .ForMember(d => d.Id, o => o.Ignore())     // defensive system 
      .ForMember(d => d.CompanyId, o => o.Ignore())// defensive system 
      .ForMember(d => d.EmployeeAccountId, o => o.Ignore());// defensive system 



        CreateMap<Contract, ContractListItemDto>();

       
        CreateMap<UpdateContractRequestDto, Contract>();


        CreateMap<Contract, ContractListItemDto>();

        // ContractListItemDto -> Contract (SEARCH FILTER ENTITY)
        CreateMap<ContractListItemDto, Contract>()
           .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.EmployeeAccountId, o => o.Ignore());

        // Contract -> ContractListItemDto (SEARCH RESULT)
        CreateMap<Contract, ContractListItemDto>();
    }
}

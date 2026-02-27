
using AutoMapper;

using ContractManagementSystem.Business.DTOs.Account;
using ContractManagementSystem.Business.DTOs.Company;
using ContractManagementSystem.Shared.Models.Account;
using ContractManagementSystem.Shared.Models.Company;
using ContractManagementSystem.Shared.Models.Company.Contract;
using ContractManagementSystem.Shared.Models.Employee;

using ContractManagementSystem.Business.DTOs.Company.Contract;
using ContractManagementSystem.Business.DTOs.Company.Employee;

namespace ContractManagementSystem.API.Mapping;

public sealed class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // SignUp
        CreateMap<SignUpRequestModel, SignUpRequestDto>();
      
        // SignIn
        CreateMap<SignInRequestModel, SignInRequestDto>();
        CreateMap<SignInResponseDto, RefreshTokenResponseModel>();

        // Refresh 
        CreateMap<RefreshTokenRequestModel, RefreshTokenRequestDto>();
        CreateMap<RefreshTokenResponseDto, RefreshTokenResponseModel>();

        // API → Business
        CreateMap<CreateCompanyRequestModel, CreateCompanyRequestDto>();

        // Business → API
        CreateMap<CreateCompanyResponseDto, CreateCompanyResponseModel>();

        // Employee registration
        CreateMap<RegisterEmployeeRequestModel, RegisterEmployeeRequestDto>();

        CreateMap<RegisterEmployeeResponseDto, RegisterEmployeeResponseModel>();

        CreateMap<RefreshTokenResponseDto, RefreshTokenResponseModel>();

        // Contract
        CreateMap<CreateContractRequestModel, CreateContractRequestDto>();
       


        CreateMap<ContractListItemDto, CreateContractRequestModel>();

      
        CreateMap<ContractListItemDto, CreateContractResponseModel>();


        // 1️⃣ API → Business (Update request)
        CreateMap<UpdateContractRequestModel, UpdateContractRequestDto>();


        // 2️⃣ Business → API (Update response)
        CreateMap<ContractListItemDto, UpdateContractResponseModel>();


        CreateMap<ContractListItemDto, CreateContractResponseModel>();

        // FilterContractModel -> ContractListItemDto (SEARCH REQUEST)
        CreateMap<FilterContractModel, ContractListItemDto>()
           
            .ForMember(d => d.CompanyId, o => o.Ignore())
          
            .ForMember(d => d.Id, o => o.Ignore());

        // ContractListItemDto -> CreateContractResponseModel (SEARCH RESPONSE)
        // If your response model has fewer fields, AutoMapper will map matching names.
        CreateMap<ContractListItemDto, CreateContractResponseModel>();
    }
}

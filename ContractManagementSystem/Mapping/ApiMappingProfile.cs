
using AutoMapper;
using ContractManagementSystem.API.Models.Account;
using ContractManagementSystem.API.Models.Company;
using ContractManagementSystem.Business.DTOs.Account;
using ContractManagementSystem.Business.DTOs.Company;
using ContractManagementSystem.API.Models;
using ContractManagementSystem.Business.DTOs.Company.Contract;
using ContractManagementSystem.Business.DTOs.Company.Employee;
using ContractManagementSystem.API.Models.Company.Contract;
using ContractManagementSystem.API.Models.Company.Employee;
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

        CreateMap<RefreshTokenResponseDto, RefreshResponseModel>();

        // Contract
        CreateMap<CreateContractRequestModel, CreateContractRequestDto>();
       


        CreateMap<ContractListItemDto, CreateContractRequestModel>();

      
        CreateMap<ContractListItemDto, CreateContractResponseModel>();


        // 1️⃣ API → Business (Update request)
        CreateMap<UpdateContractRequestModel, UpdateContractRequestDto>();


        // 2️⃣ Business → API (Update response)
        CreateMap<ContractListItemDto, UpdateContractResponseModel>();
         


    
    }
}


using AutoMapper;
using ContractManagementSystem.API.Models.Account;
using ContractManagementSystem.API.Models.Company;
using ContractManagementSystem.Business.DTOs.Account;
using ContractManagementSystem.Business.DTOs.Company;

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


    }
}

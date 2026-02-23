
using AutoMapper;
using ContractManagementSystem.API.Models.Account;
using ContractManagementSystem.Business.DTOs.Account;

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

        // Refresh (if you have it)
        CreateMap<RefreshTokenRequestModel, RefreshTokenRequestDto>();
        CreateMap<RefreshTokenResponseDto, RefreshTokenResponseModel>();
    }
}

using ContractManagementSystem.Business.DTOs.Account;

namespace ContractManagementSystem.Business.Interfaces;

public interface IAccountService
{
    Task<int> SignUpAsync(SignUpRequestDto request);
    Task<SignInResponseDto> SignInAsync(SignInRequestDto request);
    Task<RefreshTokenResponseDto> ValidateRefreshTokenAsync(RefreshTokenRequestDto requestDto);
}

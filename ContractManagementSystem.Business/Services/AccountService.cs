using AutoMapper;
using ContractManagementSystem.Business.Common.Security;
using ContractManagementSystem.Business.Config;
using ContractManagementSystem.Business.DTOs.Account;
using ContractManagementSystem.Business.Interfaces;
using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContractManagementSystem.Business.Services;

public class AccountService(IRefreshTokenRepository refreshTokenRepository, IAccountRepository accountRepository, IMapper mapper, IOptions<JwtSettings> jwtOptions) : IAccountService
{
    private readonly JwtSettings jwtSettings = jwtOptions.Value;

    public async Task<int> SignUpAsync(SignUpRequestDto requestDto)
    {

        var normalizedEmail = requestDto.Email.Trim().ToLowerInvariant();


        var existingEntity = await accountRepository.GetByEmailAsync(normalizedEmail);

        if (existingEntity is not null)
            throw new InvalidOperationException("Email already exists.");

        var account = mapper.Map<Account>(requestDto);

        account.Email = normalizedEmail;
        account.PasswordHash = PasswordHasher.Hash(requestDto.Password);

        var rows = await accountRepository.CreateAsync(account);
        if (rows != 1)
            throw new Exception("Failed to create account.");


        return rows;
    }

    public async Task<SignInResponseDto> SignInAsync(SignInRequestDto requestDto)
    {

        var normalizedEmail = requestDto.Email.Trim().ToLowerInvariant();

        var accountEntity = await accountRepository.GetByEmailAsync(normalizedEmail) ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!PasswordHasher.Verify(requestDto.Password, accountEntity.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var accessToken = GenerateAccessToken(accountEntity.Id);
        string refreshToken;

        var existingRefreshToken = await refreshTokenRepository.GetActiveByAccountIdAsync(accountEntity.Id);


        if (existingRefreshToken != null && !existingRefreshToken.IsExpired && !existingRefreshToken.IsRevoked)
        {
            refreshToken = existingRefreshToken.TokenHash;
        }
        else
        {
            refreshToken = GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                TokenHash = refreshToken,
                AccountId = accountEntity.Id
            };
            await refreshTokenRepository.SaveAsync(refreshTokenEntity);
       
        }
       
 
        return new SignInResponseDto { AccessToken = accessToken, RefreshToken = refreshToken};

    }

    public async Task<RefreshTokenResponseDto> ValidateRefreshTokenAsync(RefreshTokenRequestDto requestDto)
    {
        var oldRefreshToken = await refreshTokenRepository.GetByTokenHashId(requestDto.RefreshToken);

        if (oldRefreshToken is null || oldRefreshToken.IsExpired || oldRefreshToken.IsRevoked)
        {
            throw new UnauthorizedAccessException(
            "Refresh token is invalid. Please sign in again.");
        }

        oldRefreshToken.Revoke();

        var newRefreshToken = new RefreshToken
        {
            TokenHash = GenerateRefreshToken(),
            AccountId = oldRefreshToken.AccountId,
        };

        await refreshTokenRepository.SaveAsync(newRefreshToken);

        string newAccessToken = GenerateAccessToken(newRefreshToken.AccountId);


        var responseDto = mapper.Map<RefreshTokenResponseDto>(newRefreshToken);
        responseDto.AccessToken = newAccessToken;
        return responseDto;
    }
    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateAccessToken(Guid userId)
    {
        var claims = new[]
        {
              new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
              new Claim(ClaimTypes.Role, "User")
            };
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
          issuer: jwtSettings.Issuer,
          audience: jwtSettings.Audience,
          claims: claims,
          expires: DateTime.Now.AddHours(1),
          signingCredentials: credentials
      );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

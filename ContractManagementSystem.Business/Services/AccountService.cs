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
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContractManagementSystem.Business.Services;

public class AccountService(IRoleRepository roleRepository, IAccountRoleRepository accountRoleRepository, IRefreshTokenRepository refreshTokenRepository, IAccountRepository accountRepository, IMapper mapper, IOptions<JwtSettings> jwtOptions) : IAccountService
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

        var employeeRoleId = await roleRepository.GetIdByNameAsync("Employee") ?? throw new Exception("Role not found.");

        var accountRole = new AccountRole
        {
            AccountId = account.Id,
            RoleId = employeeRoleId,
            CreatedAtUtc = DateTime.UtcNow
        };
        var roleRows = await accountRoleRepository.AddAsync(accountRole);
        if (roleRows != 1)
            throw new Exception("Failed to assign role.");

        return rows;
 
    }

    public async Task<SignInResponseDto> SignInAsync(SignInRequestDto requestDto)
    {
        var normalizedEmail = requestDto.Email.Trim().ToLowerInvariant();

        var accountEntity = await accountRepository.GetByEmailAsync(normalizedEmail)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!PasswordHasher.Verify(requestDto.Password, accountEntity.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await accountRepository.GetRoleNamesByAccountIdAsync(accountEntity.Id);
        var accessToken = GenerateAccessToken(accountEntity.Id, roles);

        // OPTIONAL: revoke existing active token(s) for single-session behavior
        var existing = await refreshTokenRepository.GetActiveByAccountIdAsync(accountEntity.Id);
        if (existing is not null && !existing.IsExpired && !existing.IsRevoked)
        {
            existing.Revoke();
            await refreshTokenRepository.RevokeAsync(existing.Id, existing.RevokedAtUtc!.Value); // must update row
        }

        var refreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            TokenHash = refreshToken,
            AccountId = accountEntity.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };

        await refreshTokenRepository.SaveAsync(refreshTokenEntity);

        return new SignInResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<RefreshTokenResponseDto> ValidateRefreshTokenAsync(RefreshTokenRequestDto requestDto)
    {
        var oldRefreshToken = await refreshTokenRepository.GetByTokenHashId(requestDto.RefreshToken);

        if (oldRefreshToken is null || oldRefreshToken.IsExpired || oldRefreshToken.IsRevoked)
            throw new UnauthorizedAccessException("Refresh token is invalid. Please sign in again.");

        // revoke old and persist it
        oldRefreshToken.Revoke();
        await refreshTokenRepository.RevokeAsync(oldRefreshToken.Id, oldRefreshToken.RevokedAtUtc!.Value); // must update row

        // create new
        var newTokenValue = GenerateRefreshToken();
        var now = DateTime.UtcNow;

        var newRefreshToken = new RefreshToken
        {
            TokenHash = newTokenValue,
            AccountId = oldRefreshToken.AccountId,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(30)
        };

        await refreshTokenRepository.SaveAsync(newRefreshToken);

        var roles = await accountRepository.GetRoleNamesByAccountIdAsync(oldRefreshToken.AccountId);
        var newAccessToken = GenerateAccessToken(oldRefreshToken.AccountId, roles);

        return new RefreshTokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newTokenValue
        };
    }
    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateAccessToken(Guid userId, List<string> roles)
    {
        var claims = new List<Claim>
        {
              new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
              //new Claim(ClaimTypes.Role, "User")
            };

        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            claims.Add(new Claim(ClaimTypes.Role, role));

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

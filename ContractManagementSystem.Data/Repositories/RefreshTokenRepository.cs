using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Interfaces.Common;
using System.Data;

namespace ContractManagementSystem.Data.Repositories;

public class RefreshTokenRepository(IAppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<int> InsertAsync(RefreshToken row)
    {
        const string sql = @"
            INSERT INTO RefreshTokens
            (Id, AccountId, TokenHash, ExpiresAtUtc, RevokedAtUtc, CreatedAtUtc)
            VALUES
            (@Id, @AccountId, @TokenHash, @ExpiresAtUtc, @RevokedAtUtc, @CreatedAtUtc);";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = row.Id;
        command.Parameters.Add("@AccountId", SqlDbType.UniqueIdentifier).Value = row.AccountId;
        command.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 200).Value = row.TokenHash;
        command.Parameters.Add("@ExpiresAtUtc", SqlDbType.DateTime2).Value = row.ExpiresAtUtc;
        command.Parameters.Add("@RevokedAtUtc", SqlDbType.DateTime2)
            .Value = (object?)row.RevokedAtUtc ?? DBNull.Value;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = row.CreatedAtUtc;

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<RefreshToken?> GetByTokenHashId(string refreshToken)
    {
        const string sql = @"
            SELECT TOP 1 Id, AccountId, TokenHash, ExpiresAtUtc,
                   RevokedAtUtc, CreatedAtUtc
            FROM RefreshTokens
            WHERE TokenHash = @TokenHash 
            ORDER BY CreatedAtUtc DESC;";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 200).Value = refreshToken;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new RefreshToken
        {
            Id = reader.GetGuid(0),
            AccountId = reader.GetGuid(1),
            TokenHash = reader.GetString(2),
            ExpiresAtUtc = reader.GetDateTime(3),
            RevokedAtUtc = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            CreatedAtUtc = reader.GetDateTime(5)
        };
    }
    public async Task<RefreshToken?> GetActiveByAccountIdAsync(Guid accountId)
    {
        const string sql = @"
        SELECT TOP 1 Id, AccountId, TokenHash, ExpiresAtUtc,
               RevokedAtUtc, CreatedAtUtc
        FROM RefreshTokens
        WHERE AccountId = @AccountId
          AND RevokedAtUtc IS NULL
          AND ExpiresAtUtc > @Now
        ORDER BY CreatedAtUtc DESC;";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@AccountId", SqlDbType.UniqueIdentifier).Value = accountId;
        command.Parameters.Add("@Now", SqlDbType.DateTime2).Value = DateTime.UtcNow;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);

        if (!await reader.ReadAsync())
            return null;

        return new RefreshToken
        {
            Id = reader.GetGuid(0),
            AccountId = reader.GetGuid(1),
            TokenHash = reader.GetString(2),
            ExpiresAtUtc = reader.GetDateTime(3),
            RevokedAtUtc = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            CreatedAtUtc = reader.GetDateTime(5)
        };
    }
    public async Task<int> RevokeAsync(Guid tokenId, DateTime revokedAtUtc)
    {
        const string sql = @"
            UPDATE RefreshTokens
            SET RevokedAtUtc = @RevokedAtUtc
            WHERE Id = @Id;";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = tokenId;
        command.Parameters.Add("@RevokedAtUtc", SqlDbType.DateTime2).Value = revokedAtUtc;

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> SaveAsync(RefreshToken token)
    {
        const string sql = @"
        INSERT INTO RefreshTokens
        (Id, AccountId, TokenHash, ExpiresAtUtc, RevokedAtUtc, CreatedAtUtc)
        VALUES
        (@Id, @AccountId, @TokenHash, @ExpiresAtUtc, @RevokedAtUtc, @CreatedAtUtc);";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = token.Id;
        command.Parameters.Add("@AccountId", SqlDbType.UniqueIdentifier).Value = token.AccountId;
        command.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 200).Value = token.TokenHash;
        command.Parameters.Add("@ExpiresAtUtc", SqlDbType.DateTime2).Value = token.ExpiresAtUtc;
        command.Parameters.Add("@RevokedAtUtc", SqlDbType.DateTime2)
            .Value = (object?)token.RevokedAtUtc ?? DBNull.Value;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = token.CreatedAtUtc;

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }
}

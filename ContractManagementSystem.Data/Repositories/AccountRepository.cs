using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Enums;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Interfaces.Common;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ContractManagementSystem.Data.Repositories;

public class AccountRepository(IAppDbContext dbContext): IAccountRepository
{
    public async Task<int> CreateAsync(Account account)
    {
        const string sql = @"
            INSERT INTO Accounts (Id, Email, PasswordHash, CreatedAtUtc)
            VALUES (@Id, @Email, @PasswordHash, @CreatedAtUtc);";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = account.Id;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = account.Email;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = account.PasswordHash;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = account.CreatedAtUtc;
    

        await connection.OpenAsync();
        var rows = await command.ExecuteNonQueryAsync();

        return rows;
    }
    public async Task<Account?> GetByEmailAsync(string email)
    {
        const string sql = @"
        SELECT TOP 1 Id, Email, PasswordHash, CreatedAtUtc
        FROM Accounts
        WHERE Email = @Email;";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);

        if (!await reader.ReadAsync())
            return null;

        return new Account
        {
            Id = reader.GetGuid(0),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            CreatedAtUtc = reader.GetDateTime(3)
        };
    }
    public async Task<int> SaveAsync(Account account)
    {
        const string sql = @"
        INSERT INTO Accounts 
        (Id, Email, PasswordHash, CreatedAtUtc)
        VALUES 
        (@Id, @Email, @PasswordHash, @CreatedAtUtc);";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = account.Id;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = account.Email;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = account.PasswordHash;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = account.CreatedAtUtc;

        await connection.OpenAsync();
        var rows = await command.ExecuteNonQueryAsync();

       return await command.ExecuteNonQueryAsync();

    }

    public async Task<List<string>> GetRoleNamesByAccountIdAsync(Guid accountId)
    {
        const string sql = @"
        SELECT r.Name
        FROM AccountRoles ar
        INNER JOIN Roles r ON r.Id = ar.RoleId
        WHERE ar.AccountId = @AccountId;";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@AccountId", SqlDbType.UniqueIdentifier).Value = accountId;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var roles = new List<string>();
        while (await reader.ReadAsync())
        {
            roles.Add(reader.GetString(0));
        }

        return roles;
    }

}

using ContractManagementSystem.Data.Common;
using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Interfaces.Common;
using System.Data;

namespace ContractManagementSystem.Data.Repositories;

public class AccountRepository(IAppDbContext dbContext) : IAccountRepository
{
    public async Task<int> CreateAsync(Account account)
    {
        const string sql = @"
            INSERT INTO Accounts (Id, Email, PasswordHash, CreatedAtUtc, FirstName, LastName)
            VALUES (@Id, @Email, @PasswordHash, @CreatedAtUtc, @FirstName, @LastName);";

        using var connection = dbContext.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;

        DbCommandHelpers.AddParam(cmd, "@Id", DbType.Guid, account.Id);
        DbCommandHelpers.AddParam(cmd, "@Email", DbType.String, account.Email);
        DbCommandHelpers.AddParam(cmd, "@PasswordHash", DbType.String, account.PasswordHash);
        DbCommandHelpers.AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, account.CreatedAtUtc);
        DbCommandHelpers.AddParam(cmd, "@FirstName", DbType.String, account.FirstName);
        DbCommandHelpers.AddParam(cmd, "@LastName", DbType.String, account.LastName);


        await connection.OpenAsync();
        var rows = await cmd.ExecuteNonQueryAsync();

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

        DbCommandHelpers.AddParam(command, "@Email", DbType.String, email, 256);

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
        (Id, Email, PasswordHash, CreatedAtUtc, FirstName, LastName)
        VALUES
        (@Id, @Email, @PasswordHash, @CreatedAtUtc, @FirstName, @LastName);";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        DbCommandHelpers.AddParam(command, "@Id", DbType.Guid, account.Id);
        DbCommandHelpers.AddParam(command, "@Email", DbType.String, account.Email, 256);
        DbCommandHelpers.AddParam(command, "@PasswordHash", DbType.String, account.PasswordHash, 500);
        DbCommandHelpers.AddParam(command, "@CreatedAtUtc", DbType.DateTime2, account.CreatedAtUtc);
        DbCommandHelpers.AddParam(command, "@FirstName", DbType.String, account.FirstName, 100);
        DbCommandHelpers.AddParam(command, "@LastName", DbType.String, account.LastName, 100);
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

        DbCommandHelpers.AddParam(command, "@AccountId", DbType.Guid, accountId);

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

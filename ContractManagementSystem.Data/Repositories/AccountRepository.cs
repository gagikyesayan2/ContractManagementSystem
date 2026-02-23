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
            INSERT INTO Accounts (Id, Email, PasswordHash, AccountType, CreatedAtUtc)
            VALUES (@Id, @Email, @PasswordHash, @AccountType, @CreatedAtUtc);";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = account.Id;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = account.Email;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = account.PasswordHash;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = account.CreatedAtUtc;
        command.Parameters.Add("@AccountType", SqlDbType.Int).Value = (int)account.AccountType;

        await connection.OpenAsync();
        var rows = await command.ExecuteNonQueryAsync();

        return rows;
    }
    public async Task<Account?> GetByEmailAsync(string email)
    {
        const string sql = @"
        SELECT TOP 1 Id, Email, PasswordHash, AccountType, CreatedAtUtc
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
            AccountType = (AccountTypeEnum)reader.GetInt32(3),
            CreatedAtUtc = reader.GetDateTime(4)
        };
    }
    public async Task<int> SaveAsync(Account account)
    {
        const string sql = @"
        INSERT INTO Accounts 
        (Id, Email, PasswordHash, AccountType, CreatedAtUtc)
        VALUES 
        (@Id, @Email, @PasswordHash, @AccountType, @CreatedAtUtc);";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = account.Id;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = account.Email;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = account.PasswordHash;
        command.Parameters.Add("@AccountType", SqlDbType.Int).Value = (int)account.AccountType;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = account.CreatedAtUtc;

        await connection.OpenAsync();
        var rows = await command.ExecuteNonQueryAsync();

       return await command.ExecuteNonQueryAsync();

    }

   

}

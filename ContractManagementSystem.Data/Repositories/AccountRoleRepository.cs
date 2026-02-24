using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Interfaces.Common;
using System.Data;

namespace ContractManagementSystem.Data.Repositories;

public sealed class AccountRoleRepository(IAppDbContext dbContext) : IAccountRoleRepository
{
    public async Task<int> AddAsync(AccountRole entity)
    {
        const string sql = @"
        INSERT INTO AccountRoles (AccountId, RoleId, CreatedAtUtc)
        VALUES (@AccountId, @RoleId, @CreatedAtUtc);";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@AccountId", SqlDbType.UniqueIdentifier).Value = entity.AccountId;
        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = entity.RoleId;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = entity.CreatedAtUtc;

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }
}

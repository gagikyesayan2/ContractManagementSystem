using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Interfaces.Common;
using System.Data;

namespace ContractManagementSystem.Data.Repositories;

public sealed class RoleRepository(IAppDbContext dbContext) : IRoleRepository
{
    public async Task<int?> GetIdByNameAsync(string name)
    {
        const string sql = @"SELECT TOP 1 Id FROM Roles WHERE Name = @Name;";

        using var connection = dbContext.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result is null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }
}

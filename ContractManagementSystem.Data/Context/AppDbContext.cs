using ContractManagementSystem.Data.Interfaces.Common;
using Microsoft.Data.SqlClient;

namespace ContractManagementSystem.Data.Context;

public sealed class AppDbContext : IAppDbContext
{
    private readonly string _cs;
    public AppDbContext(string connectionString) => _cs = connectionString;

    public SqlConnection CreateConnection() => new SqlConnection(_cs);
}

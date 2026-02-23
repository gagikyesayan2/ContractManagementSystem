using Microsoft.Data.SqlClient;

namespace ContractManagementSystem.Data.Interfaces.Common;

public interface IAppDbContext
{
    SqlConnection CreateConnection();
}


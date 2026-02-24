

namespace ContractManagementSystem.Data.Interfaces;

public interface IRoleRepository
{
    Task<int?> GetIdByNameAsync(string name);
}

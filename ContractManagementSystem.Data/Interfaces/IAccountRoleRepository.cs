using ContractManagementSystem.Data.Entities;

namespace ContractManagementSystem.Data.Interfaces;

public interface IAccountRoleRepository
{
    Task<int> AddAsync(AccountRole entity);
}

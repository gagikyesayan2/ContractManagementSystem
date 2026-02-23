using ContractManagementSystem.Data.Entities;

namespace ContractManagementSystem.Data.Interfaces;

public interface IAccountRepository
{
    Task<int> CreateAsync(Account account);
    Task<Account?> GetByEmailAsync(string email);
    Task<int> SaveAsync(Account account);
}


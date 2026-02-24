using ContractManagementSystem.Data.Entities;
using Microsoft.Data.SqlClient;

namespace ContractManagementSystem.Data.Interfaces;

public interface ICompanyRepository
{
    Task<Company> CreateWithAdminAsync(Company company, Guid AccountId, CancellationToken ct = default);

    Task<Guid> AddEmployeeAsync(Guid companyId, string employeeEmail, string employeePasswordHash, Guid createdByAdminAccountId, CancellationToken ct = default);
}

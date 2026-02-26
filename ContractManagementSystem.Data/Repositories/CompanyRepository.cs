using ContractManagementSystem.Data.Common;
using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Interfaces.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
namespace ContractManagementSystem.Data.Repositories;

public sealed class CompanyRepository(IAppDbContext dbContext) : ICompanyRepository
{

public async Task<bool> IsValidEmployeeInCompany(Guid companyId, Guid accountId)
{
        const string sql = @"
            SELECT 1
            FROM CompanyAccounts
            WHERE CompanyId = @CompanyId
              AND AccountId = @AccountId;";

        await using var connection = dbContext.CreateConnection();
        await connection.OpenAsync();

        var result = await DbCommandHelpers.ExecuteScalarAsync(
            connection,
            tx: null,
            sql: sql,
            ct: CancellationToken.None,
            configure: cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                DbCommandHelpers.AddParam(cmd, "@AccountId", DbType.Guid, accountId);
            });

        return result != null;
    }


public async Task<bool> IsAdminAsync(Guid companyId, Guid accountId, CancellationToken ct)
    {
        const string sql = @"
    SELECT 1
    FROM CompanyAccounts ca
    JOIN AccountRoles ar ON ar.AccountId = ca.AccountId
    JOIN Roles r ON r.Id = ar.RoleId
    WHERE ca.CompanyId = @CompanyId
      AND ca.AccountId = @AccountId
      AND r.Name = 'Admin';
    ";

        await using var connection = dbContext.CreateConnection();
        await connection.OpenAsync(ct);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;

        DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
        DbCommandHelpers.AddParam(cmd, "@AccountId", DbType.Guid, accountId);

        var result = await cmd.ExecuteScalarAsync(ct);

        return result != null;
    }
public async Task<Guid> AddEmployeeAsync(Guid companyId,string employeeEmail,string employeePasswordHash,Guid createdByAdminAccountId, string firstName, string lastName, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var employeeAccountId = Guid.NewGuid();

            await using DbConnection connection = dbContext.CreateConnection();
            await connection.OpenAsync(ct);

            await using DbTransaction tx =
                await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                // 0) Ensure caller is admin of this company (DB-level check)
                // If you already check in Business via repo queries, you can remove this section.
                const string ensureAdminSql = @"
                IF NOT EXISTS (
                SELECT 1
                FROM CompanyAccounts ca
                JOIN AccountRoles ar ON ar.AccountId = ca.AccountId
                JOIN Roles r ON r.Id = ar.RoleId
                WHERE ca.CompanyId = @CompanyId
                      AND ca.AccountId = @AdminAccountId
                      AND r.Name = @AdminRoleName
                )
            THROW 50001, 'Only company admin can register employees.', 1;";

                await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, ensureAdminSql, ct, cmd =>
                {
                    DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                    DbCommandHelpers.AddParam(cmd, "@AdminAccountId", DbType.Guid, createdByAdminAccountId);
                    DbCommandHelpers.AddParam(cmd, "@AdminRoleName", DbType.String, "Admin");
                });

                // 1) Insert Accounts (employee)
                // relies on UNIQUE constraint on Accounts.Email
                const string insertAccountSql = @"
                INSERT INTO Accounts (Id, Email, PasswordHash, CreatedAtUtc, FirstName, LastName)
                VALUES (@Id, @Email, @PasswordHash, @CreatedAtUtc, @FirstName, @LastName);";

                await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, insertAccountSql, ct, cmd =>
                {
                    DbCommandHelpers.AddParam(cmd, "@Id", DbType.Guid, employeeAccountId);
                    DbCommandHelpers.AddParam(cmd, "@Email", DbType.String, employeeEmail);
                    DbCommandHelpers.AddParam(cmd, "@PasswordHash", DbType.String, employeePasswordHash);
                    DbCommandHelpers.AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, now);
                    DbCommandHelpers.AddParam(cmd, "@FirstName", DbType.String, firstName);
                    DbCommandHelpers.AddParam(cmd, "@LastName", DbType.String, lastName);
                });

                // 2) Insert CompanyAccounts
                const string insertCompanyAccountSql = @"
                INSERT INTO CompanyAccounts (CompanyId, AccountId, JoinedAtUtc)
                VALUES (@CompanyId, @AccountId, @JoinedAtUtc);";

                await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, insertCompanyAccountSql, ct, cmd =>
                {
                    DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                    DbCommandHelpers.AddParam(cmd, "@AccountId", DbType.Guid, employeeAccountId);
                    DbCommandHelpers.AddParam(cmd, "@JoinedAtUtc", DbType.DateTime2, now);
                });

                // 3) Get Employee RoleId
                const string getEmployeeRoleSql = @"
                    SELECT TOP (1) Id
                    FROM Roles
                    WHERE Name = @Name;";

                var roleIdObj = await DbCommandHelpers.ExecuteScalarAsync(connection, tx, getEmployeeRoleSql, ct, cmd =>
                {
                    DbCommandHelpers.AddParam(cmd, "@Name", DbType.String, "Employee");
                });

                if (roleIdObj is null || roleIdObj == DBNull.Value)
                    throw new InvalidOperationException("Role 'Employee' not found. Seed the Roles table.");

                var employeeRoleId = Convert.ToInt32(roleIdObj);

                // 4) Insert AccountRoles (Employee)
                const string insertAccountRoleSql = @"
                    INSERT INTO AccountRoles (AccountId, RoleId, CreatedAtUtc)
                    VALUES (@AccountId, @RoleId, @CreatedAtUtc);";

                await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, insertAccountRoleSql, ct, cmd =>
                {
                    DbCommandHelpers.AddParam(cmd, "@AccountId", DbType.Guid, employeeAccountId);
                    DbCommandHelpers.AddParam(cmd, "@RoleId", DbType.Int32, employeeRoleId);
                    DbCommandHelpers.AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, now);
                });

                await tx.CommitAsync(ct);
                return employeeAccountId;
            }
            catch (DbException ex) when (IsUniqueViolation(ex))
            {
                await tx.RollbackAsync(ct);
                // map to your custom exception if you have one
                throw new InvalidOperationException("Email already exists.", ex);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

public async Task<Company> CreateWithAdminAsync(Company company, Guid creatorAccountId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var companyId = Guid.NewGuid();

        await using var connection = dbContext.CreateConnection();
        await connection.OpenAsync(ct);

        await using var tx =
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        try
        {
            // 1️ Insert Company
            const string insertCompanySql = @"
        INSERT INTO Companies (Id, Name, CreatedAtUtc)
        VALUES (@Id, @Name, @CreatedAtUtc);";

            await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, insertCompanySql, ct, cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@Id", DbType.Guid, companyId);
                DbCommandHelpers.AddParam(cmd, "@Name", DbType.String, company.Name);
                DbCommandHelpers.AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, now);
            });

            // 2️ Insert CompanyAccounts
            const string insertCompanyAccountSql = @"
INSERT INTO CompanyAccounts (CompanyId, AccountId, JoinedAtUtc)
VALUES (@CompanyId, @AccountId, @JoinedAtUtc);";

            await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, insertCompanyAccountSql, ct, cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                DbCommandHelpers.AddParam(cmd, "@AccountId", DbType.Guid, creatorAccountId);
                DbCommandHelpers.AddParam(cmd, "@JoinedAtUtc", DbType.DateTime2, now);
            });

            // 3️ Get Admin RoleId
            const string getRoleSql = @"
SELECT TOP (1) Id FROM Roles WHERE Name = @Name;";

            var roleIdObj = await DbCommandHelpers.ExecuteScalarAsync(connection, tx, getRoleSql, ct, cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@Name", DbType.String, "Admin");
            });

            var adminRoleId = Convert.ToInt32(roleIdObj);

            // 4️ Insert AccountRoles
            const string insertAccountRoleSql = @"IF NOT EXISTS (
                SELECT 1
                FROM AccountRoles
                WHERE AccountId = @AccountId AND RoleId = @RoleId
            )
            BEGIN
                INSERT INTO AccountRoles (AccountId, RoleId, CreatedAtUtc)
                VALUES (@AccountId, @RoleId, @CreatedAtUtc);
            END";

            await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, insertAccountRoleSql, ct, cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@AccountId", DbType.Guid, creatorAccountId);
                DbCommandHelpers.AddParam(cmd, "@RoleId", DbType.Int32, adminRoleId);
                DbCommandHelpers.AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, now);
            });

            await tx.CommitAsync(ct);

            company.Id = companyId;
            company.CreatedAtUtc = now;

            return company;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

private static bool IsUniqueViolation(DbException ex)
        => ex is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
}
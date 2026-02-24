using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Interfaces.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace ContractManagementSystem.Data.Repositories;

public sealed class CompanyRepository(IAppDbContext dbContext) : ICompanyRepository
{
    
        public async Task<Guid> AddEmployeeAsync(Guid companyId,string employeeEmail,string employeePasswordHash,Guid createdByAdminAccountId,CancellationToken ct = default)
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

                await ExecuteNonQueryAsync(connection, tx, ensureAdminSql, ct, cmd =>
                {
                    AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                    AddParam(cmd, "@AdminAccountId", DbType.Guid, createdByAdminAccountId);
                    AddParam(cmd, "@AdminRoleName", DbType.String, "Admin");
                });

                // 1) Insert Accounts (employee)
                // relies on UNIQUE constraint on Accounts.Email
                const string insertAccountSql = @"
                INSERT INTO Accounts (Id, Email, PasswordHash, CreatedAtUtc)
                VALUES (@Id, @Email, @PasswordHash, @CreatedAtUtc);";

                await ExecuteNonQueryAsync(connection, tx, insertAccountSql, ct, cmd =>
                {
                    AddParam(cmd, "@Id", DbType.Guid, employeeAccountId);
                    AddParam(cmd, "@Email", DbType.String, employeeEmail);
                    AddParam(cmd, "@PasswordHash", DbType.String, employeePasswordHash);
                    AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, now);
                });

                // 2) Insert CompanyAccounts
                const string insertCompanyAccountSql = @"
                INSERT INTO CompanyAccounts (CompanyId, AccountId, JoinedAtUtc)
                VALUES (@CompanyId, @AccountId, @JoinedAtUtc);";

                await ExecuteNonQueryAsync(connection, tx, insertCompanyAccountSql, ct, cmd =>
                {
                    AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                    AddParam(cmd, "@AccountId", DbType.Guid, employeeAccountId);
                    AddParam(cmd, "@JoinedAtUtc", DbType.DateTime2, now);
                });

                // 3) Get Employee RoleId
                const string getEmployeeRoleSql = @"
                    SELECT TOP (1) Id
                    FROM Roles
                    WHERE Name = @Name;";

                var roleIdObj = await ExecuteScalarAsync(connection, tx, getEmployeeRoleSql, ct, cmd =>
                {
                    AddParam(cmd, "@Name", DbType.String, "Employee");
                });

                if (roleIdObj is null || roleIdObj == DBNull.Value)
                    throw new InvalidOperationException("Role 'Employee' not found. Seed the Roles table.");

                var employeeRoleId = Convert.ToInt32(roleIdObj);

                // 4) Insert AccountRoles (Employee)
                const string insertAccountRoleSql = @"
                    INSERT INTO AccountRoles (AccountId, RoleId, CreatedAtUtc)
                    VALUES (@AccountId, @RoleId, @CreatedAtUtc);";

                await ExecuteNonQueryAsync(connection, tx, insertAccountRoleSql, ct, cmd =>
                {
                    AddParam(cmd, "@AccountId", DbType.Guid, employeeAccountId);
                    AddParam(cmd, "@RoleId", DbType.Int32, employeeRoleId);
                    AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, now);
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

            await ExecuteNonQueryAsync(connection, tx, insertCompanySql, ct, cmd =>
            {
                AddParam(cmd, "@Id", DbType.Guid, companyId);
                AddParam(cmd, "@Name", DbType.String, company.Name);
                AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, now);
            });

            // 2️ Insert CompanyAccounts
            const string insertCompanyAccountSql = @"
INSERT INTO CompanyAccounts (CompanyId, AccountId, JoinedAtUtc)
VALUES (@CompanyId, @AccountId, @JoinedAtUtc);";

            await ExecuteNonQueryAsync(connection, tx, insertCompanyAccountSql, ct, cmd =>
            {
                AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                AddParam(cmd, "@AccountId", DbType.Guid, creatorAccountId);
                AddParam(cmd, "@JoinedAtUtc", DbType.DateTime2, now);
            });

            // 3️ Get Admin RoleId
            const string getRoleSql = @"
SELECT TOP (1) Id FROM Roles WHERE Name = @Name;";

            var roleIdObj = await ExecuteScalarAsync(connection, tx, getRoleSql, ct, cmd =>
            {
                AddParam(cmd, "@Name", DbType.String, "Admin");
            });

            var adminRoleId = Convert.ToInt32(roleIdObj);

            // 4️ Insert AccountRoles
            const string insertAccountRoleSql = @"
INSERT INTO AccountRoles (AccountId, RoleId, CreatedAtUtc)
VALUES (@AccountId, @RoleId, @CreatedAtUtc);";

            await ExecuteNonQueryAsync(connection, tx, insertAccountRoleSql, ct, cmd =>
            {
                AddParam(cmd, "@AccountId", DbType.Guid, creatorAccountId);
                AddParam(cmd, "@RoleId", DbType.Int32, adminRoleId);
                AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, now);
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

private static async Task<int> ExecuteNonQueryAsync(DbConnection connection,DbTransaction tx,string sql,CancellationToken ct,Action<DbCommand> configure)
    {
        await using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;

        configure(cmd);

        return await cmd.ExecuteNonQueryAsync(ct);
    }

private static async Task<object?> ExecuteScalarAsync(DbConnection connection,DbTransaction tx,string sql,CancellationToken ct,Action<DbCommand> configure)
    {
        await using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;

        configure(cmd);

        return await cmd.ExecuteScalarAsync(ct);
    }

private static void AddParam(DbCommand command, string name, DbType type, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value;

        command.Parameters.Add(parameter);
    }

private static bool IsUniqueViolation(DbException ex)
        => ex is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
}
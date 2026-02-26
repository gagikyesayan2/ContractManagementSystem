using ContractManagementSystem.Data.Common;
using ContractManagementSystem.Data.Entities;
using ContractManagementSystem.Data.Enums;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Interfaces.Common;
using System.Data;
using System.Data.Common;

namespace ContractManagementSystem.Data.Repositories;

public class ContractRepository (IAppDbContext dbContext): IContractRepository
{
    public async Task<bool> DeleteAsync(Guid contractId, Guid companyId, CancellationToken ct)
    {
        const string sql = @"
                DELETE FROM Contracts
                WHERE Id = @ContractId
                  AND CompanyId = @CompanyId;
                ";

        await using var connection = dbContext.CreateConnection();
        await connection.OpenAsync(ct);

        var affected = await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx: null, sql, ct, cmd =>
        {
            DbCommandHelpers.AddParam(cmd, "@ContractId", DbType.Guid, contractId);
            DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
        });

        return affected > 0;
    }
    public async Task<Contract> UpdateAsync(Contract contract,Guid editedByAdminAccountId,CancellationToken ct)
    {
        using var connection = dbContext.CreateConnection();
        await connection.OpenAsync(ct);

        using var tx = connection.BeginTransaction();

        try
        {
            // 1) Get CompanyId for the contract (and ensure contract exists)
            const string sqlGetCompanyId = @"
                SELECT CompanyId
                FROM Contracts
                WHERE Id = @ContractId;";

            var companyIdObj = await DbCommandHelpers.ExecuteScalarAsync(connection, tx, sqlGetCompanyId, ct,
                cmd => DbCommandHelpers.AddParam(cmd, "@ContractId", DbType.Guid, contract.Id)
            );

            if (companyIdObj is null || companyIdObj == DBNull.Value)
                throw new InvalidOperationException("Contract not found.");

            var companyId = (Guid)companyIdObj;

            // 2) Ensure editor is Admin of that company (DB-level authorization)
            const string sqlAdminCheck = @"
            IF NOT EXISTS (
                SELECT 1
                FROM CompanyAccounts ca
                JOIN AccountRoles ar ON ar.AccountId = ca.AccountId
                JOIN Roles r ON r.Id = ar.RoleId
                WHERE ca.CompanyId = @CompanyId
                  AND ca.AccountId = @EditorId
                  AND r.Name = 'Admin'
            )
            THROW 50001, 'Not authorized to edit contract.', 1;";

            await DbCommandHelpers.ExecuteNonQueryAsync(
                connection, tx, sqlAdminCheck, ct,
                cmd =>
                {
                    DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                    DbCommandHelpers.AddParam(cmd, "@EditorId", DbType.Guid, editedByAdminAccountId);
                });

            // 3) Update contract
            const string sqlUpdate = @"
            UPDATE Contracts
            SET Title = @Title,
                Description = @Description,
                EmploymentStartDate = @StartDate,
                EmploymentEndDate = @EndDate,
                Wage = @Wage
            WHERE Id = @ContractId;";

            var rows = await DbCommandHelpers.ExecuteNonQueryAsync(
                connection, tx, sqlUpdate, ct,
                cmd =>
                {
                    DbCommandHelpers.AddParam(cmd, "@Title", DbType.String, contract.Title, size: 200);
                    DbCommandHelpers.AddParam(cmd, "@Description", DbType.String, contract.Description);

                    // SQL column type is DATE, so we pass DateTime at midnight
                    DbCommandHelpers.AddParam(cmd, "@StartDate", DbType.Date,
                        contract.EmploymentStartDate.ToDateTime(TimeOnly.MinValue));

                    DbCommandHelpers.AddParam(cmd, "@EndDate", DbType.Date,
                        contract.EmploymentEndDate.HasValue
                            ? contract.EmploymentEndDate.Value.ToDateTime(TimeOnly.MinValue)
                            : null);

                    DbCommandHelpers.AddParam(cmd, "@Wage", DbType.Decimal, contract.Wage);
                    DbCommandHelpers.AddParam(cmd, "@ContractId", DbType.Guid, contract.Id);
                });

            if (rows == 0)
                throw new InvalidOperationException("Contract not found."); // safety (shouldn't happen after step 1)

            // 4) Read updated row and return it

            const string sqlSelect = @"
                SELECT Id, CompanyId, EmployeeAccountId, Title, Description,
                       EmploymentStartDate, EmploymentEndDate, Wage, CreatedAtUtc
                FROM Contracts
                WHERE Id = @ContractId;";

            var updated = await DbCommandHelpers.ExecuteReaderAsync(
                connection, tx, sqlSelect, ct,
                cmd => DbCommandHelpers.AddParam(cmd, "@ContractId", DbType.Guid, contract.Id),
                async (reader, token) =>
                {
                    if (!await reader.ReadAsync(token))
                        throw new InvalidOperationException("Contract not found after update.");

                    var startDt = reader.GetDateTime(reader.GetOrdinal("EmploymentStartDate"));
                    DateTime? endDt = reader.IsDBNull(reader.GetOrdinal("EmploymentEndDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("EmploymentEndDate"));

                    return new Contract
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        CompanyId = reader.GetGuid(reader.GetOrdinal("CompanyId")),
                        EmployeeAccountId = reader.GetGuid(reader.GetOrdinal("EmployeeAccountId")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Description")),
                        EmploymentStartDate = DateOnly.FromDateTime(startDt),
                        EmploymentEndDate = endDt is null ? (DateOnly?)null : DateOnly.FromDateTime(endDt.Value),
                        Wage = reader.GetDecimal(reader.GetOrdinal("Wage")),
                        CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
                    };
                });


            tx.Commit();

            return updated;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<Contract> AddAsync(Contract contract,Guid createdByAdminAccountId,CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        contract.Id = Guid.NewGuid();
        contract.CreatedAtUtc = now;

        await using DbConnection connection = dbContext.CreateConnection();
        await connection.OpenAsync(ct);

        await using DbTransaction tx =
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        try
        {
            // 1) Ensure caller is admin of this company
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
            THROW 50011, 'Only company admin can create contracts.', 1;";

            await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, ensureAdminSql, ct, cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, contract.CompanyId);
                DbCommandHelpers.AddParam(cmd, "@AdminAccountId", DbType.Guid, createdByAdminAccountId);
                DbCommandHelpers.AddParam(cmd, "@AdminRoleName", DbType.String, "Admin");
            });

            // 2) Ensure employee belongs to this company
            const string ensureEmployeeSql = @"
            IF NOT EXISTS (
                SELECT 1
                FROM CompanyAccounts
                WHERE CompanyId = @CompanyId
                  AND AccountId = @EmployeeAccountId
            )
            THROW 50012, 'Employee is not a member of this company.', 1;";

            await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, ensureEmployeeSql, ct, cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, contract.CompanyId);
                DbCommandHelpers.AddParam(cmd, "@EmployeeAccountId", DbType.Guid, contract.EmployeeAccountId);
            });

            // 3) Insert contract
            const string insertSql = @"
                INSERT INTO Contracts
                (Id,CompanyId,EmployeeAccountId,Title,Description,EmploymentStartDate,EmploymentEndDate,Wage,CreatedAtUtc)
                VALUES(@Id,@CompanyId,@EmployeeAccountId,@Title,@Description,@EmploymentStartDate,@EmploymentEndDate,@Wage,@CreatedAtUtc);";

            await DbCommandHelpers.ExecuteNonQueryAsync(connection, tx, insertSql, ct, cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@Id", DbType.Guid, contract.Id);
                DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, contract.CompanyId);
                DbCommandHelpers.AddParam(cmd, "@EmployeeAccountId", DbType.Guid, contract.EmployeeAccountId);
                DbCommandHelpers.AddParam(cmd, "@Title", DbType.String, contract.Title);
                DbCommandHelpers.AddParam(cmd, "@Description", DbType.String,
                    (object?)contract.Description ?? DBNull.Value);

                DbCommandHelpers.AddParam(cmd, "@EmploymentStartDate", DbType.Date,
                    contract.EmploymentStartDate.ToDateTime(TimeOnly.MinValue));

                DbCommandHelpers.AddParam(cmd, "@EmploymentEndDate", DbType.Date,
                    contract.EmploymentEndDate is null
                        ? DBNull.Value
                        : contract.EmploymentEndDate.Value.ToDateTime(TimeOnly.MinValue));
                DbCommandHelpers.AddParam(cmd, "@Wage", DbType.Decimal, contract.Wage);
                DbCommandHelpers.AddParam(cmd, "@CreatedAtUtc", DbType.DateTime2, contract.CreatedAtUtc);
            });

            // 4) Select created contract (including employee names)
            const string selectSql = @"SELECT c.Id,c.CompanyId,c.EmployeeAccountId,c.Title,
                c.Description,
                c.EmploymentStartDate,
                c.EmploymentEndDate,
                c.Wage,
                c.CreatedAtUtc,
                a.FirstName,
                a.LastName
            FROM Contracts c
            JOIN Accounts a ON a.Id = c.EmployeeAccountId
            WHERE c.Id = @ContractId;";

            var selected = await DbCommandHelpers.ExecuteReaderAsync(
                connection,
                tx,
                selectSql,
                ct,
                cmd => DbCommandHelpers.AddParam(cmd, "@ContractId", DbType.Guid, contract.Id),
                async (reader, token) =>
                {
                    if (!await reader.ReadAsync(token))
                        throw new InvalidOperationException("Failed to load created contract.");

                    var startDt = reader.GetDateTime(reader.GetOrdinal("EmploymentStartDate"));
                    DateTime? endDt = reader.IsDBNull(reader.GetOrdinal("EmploymentEndDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("EmploymentEndDate"));

                    return new Contract
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        CompanyId = reader.GetGuid(reader.GetOrdinal("CompanyId")),
                        EmployeeAccountId = reader.GetGuid(reader.GetOrdinal("EmployeeAccountId")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Description")),
                        EmploymentStartDate = DateOnly.FromDateTime(startDt),
                        EmploymentEndDate = endDt is null ? (DateOnly?)null : DateOnly.FromDateTime(endDt.Value),
                        Wage = reader.GetDecimal(reader.GetOrdinal("Wage")),
                        CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),

                        // not part of Contract entity, but we can return it here to avoid another DB call in service layer
                        EmployeeFirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        EmployeeLastName = reader.GetString(reader.GetOrdinal("LastName"))
                    };
                });

            await tx.CommitAsync(ct);
            return selected;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<Contract>> SearchAsync(Contract contract,
        CancellationToken ct = default)
    {
        // contract.CompanyId = required
        // contract.Title / EmployeeFirstName / EmployeeLastName = optional filters

        const string sql = @"
            SELECT
                c.Id,
                c.CompanyId,
                c.EmployeeAccountId,
                c.Title,
                c.Description,
                c.EmploymentStartDate,
                c.EmploymentEndDate,
                c.Wage,
                c.CreatedAtUtc,
                a.FirstName AS EmployeeFirstName,
                a.LastName  AS EmployeeLastName
            FROM Contracts c
            JOIN Accounts a ON a.Id = c.EmployeeAccountId
            WHERE c.CompanyId = @CompanyId
            AND (
                    (@Title     IS NOT NULL AND c.Title      LIKE '%' + @Title + '%')
                 OR (@FirstName IS NOT NULL AND a.FirstName  LIKE '%' + @FirstName + '%')
                 OR (@LastName  IS NOT NULL AND a.LastName   LIKE '%' + @LastName + '%')
                     )
            ORDER BY c.CreatedAtUtc DESC;";

        await using var connection = dbContext.CreateConnection();
        await connection.OpenAsync(ct);

        return await DbCommandHelpers.ExecuteReaderAsync(
            connection,
            tx: null,
            sql: sql,
            ct: ct,
            configure: cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, contract.CompanyId);

                // optional filters (can be null/empty)
                DbCommandHelpers.AddParam(cmd, "@Title", DbType.String, (object?)contract.Title ?? DBNull.Value, size: 200);
                DbCommandHelpers.AddParam(cmd, "@FirstName", DbType.String, (object?)contract.EmployeeFirstName ?? DBNull.Value, size: 100);
                DbCommandHelpers.AddParam(cmd, "@LastName", DbType.String, (object?)contract.EmployeeLastName ?? DBNull.Value, size: 100);
            },
            mapAsync: async (reader, token) =>
            {
                var results = new List<Contract>();

                // ordinals (fast + safe)
                var oId = reader.GetOrdinal("Id");
                var oCompanyId = reader.GetOrdinal("CompanyId");
                var oEmployeeAccountId = reader.GetOrdinal("EmployeeAccountId");
                var oTitle = reader.GetOrdinal("Title");
                var oDescription = reader.GetOrdinal("Description");
                var oStart = reader.GetOrdinal("EmploymentStartDate");
                var oEnd = reader.GetOrdinal("EmploymentEndDate");
                var oWage = reader.GetOrdinal("Wage");
                var oCreatedAt = reader.GetOrdinal("CreatedAtUtc");
                var oEmpFirst = reader.GetOrdinal("EmployeeFirstName");
                var oEmpLast = reader.GetOrdinal("EmployeeLastName");

                while (await reader.ReadAsync(token))
                {
                    results.Add(new Contract
                    {
                        Id = reader.GetGuid(oId),
                        CompanyId = reader.GetGuid(oCompanyId),
                        EmployeeAccountId = reader.GetGuid(oEmployeeAccountId),
                        Title = reader.GetString(oTitle),
                        Description = reader.IsDBNull(oDescription) ? null : reader.GetString(oDescription),

                        // SQL 'date' comes as DateTime (00:00:00), convert to DateOnly
                        EmploymentStartDate = DateOnly.FromDateTime(reader.GetDateTime(oStart)),
                        EmploymentEndDate = reader.IsDBNull(oEnd)
                            ? null
                            : DateOnly.FromDateTime(reader.GetDateTime(oEnd)),

                        Wage = reader.GetDecimal(oWage),
                        CreatedAtUtc = reader.GetDateTime(oCreatedAt),

                        // join-only fields
                        EmployeeFirstName = reader.IsDBNull(oEmpFirst) ? "" : reader.GetString(oEmpFirst),
                        EmployeeLastName = reader.IsDBNull(oEmpLast) ? "" : reader.GetString(oEmpLast),
                    });
                }

                return results;
            });
    }


    public async Task<IReadOnlyList<Contract>> GetAllByCompanyIdAsync(
    Guid companyId,
    CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                c.Id,
                c.CompanyId,
                c.EmployeeAccountId,
                c.Title,
                c.Description,
                c.EmploymentStartDate,
                c.EmploymentEndDate,
                c.Wage,
                c.CreatedAtUtc,
                a.FirstName AS EmployeeFirstName,
                a.LastName  AS EmployeeLastName
            FROM Contracts c
            JOIN Accounts a ON a.Id = c.EmployeeAccountId
            WHERE c.CompanyId = @CompanyId
            ORDER BY c.CreatedAtUtc DESC;";

        await using var connection = dbContext.CreateConnection();
        await connection.OpenAsync(ct);

        return await DbCommandHelpers.ExecuteReaderAsync(
            connection,
            tx: null,
            sql: sql,
            ct: ct,
            configure: cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
            },
            mapAsync: async (reader, token) =>
            {
                var results = new List<Contract>();

                var oId = reader.GetOrdinal("Id");
                var oCompanyId = reader.GetOrdinal("CompanyId");
                var oEmployeeAccountId = reader.GetOrdinal("EmployeeAccountId");
                var oTitle = reader.GetOrdinal("Title");
                var oDescription = reader.GetOrdinal("Description");
                var oStart = reader.GetOrdinal("EmploymentStartDate");
                var oEnd = reader.GetOrdinal("EmploymentEndDate");
                var oWage = reader.GetOrdinal("Wage");
                var oCreatedAt = reader.GetOrdinal("CreatedAtUtc");
                var oEmpFirst = reader.GetOrdinal("EmployeeFirstName");
                var oEmpLast = reader.GetOrdinal("EmployeeLastName");

                while (await reader.ReadAsync(token))
                {
                    results.Add(new Contract
                    {
                        Id = reader.GetGuid(oId),
                        CompanyId = reader.GetGuid(oCompanyId),
                        EmployeeAccountId = reader.GetGuid(oEmployeeAccountId),
                        Title = reader.GetString(oTitle),
                        Description = reader.IsDBNull(oDescription) ? null : reader.GetString(oDescription),

                        EmploymentStartDate = DateOnly.FromDateTime(reader.GetDateTime(oStart)),
                        EmploymentEndDate = reader.IsDBNull(oEnd)
                            ? null
                            : DateOnly.FromDateTime(reader.GetDateTime(oEnd)),

                        Wage = reader.GetDecimal(oWage),
                        CreatedAtUtc = reader.GetDateTime(oCreatedAt),

                        EmployeeFirstName = reader.IsDBNull(oEmpFirst) ? "" : reader.GetString(oEmpFirst),
                        EmployeeLastName = reader.IsDBNull(oEmpLast) ? "" : reader.GetString(oEmpLast),
                    });
                }

                return results;
            });
    }


    public async Task<IReadOnlyList<Contract>> GetByStatusAsync(Guid companyId, ContractStatus status, CancellationToken ct = default)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);

        string statusCondition = status switch
        {
            ContractStatus.NotStarted =>
                "c.EmploymentStartDate > @Now",

            ContractStatus.Active =>
                @"c.EmploymentStartDate <= @Now
              AND (c.EmploymentEndDate IS NULL
                   OR c.EmploymentEndDate >= @Now)",

            ContractStatus.Finished =>
                @"c.EmploymentEndDate IS NOT NULL
              AND c.EmploymentEndDate < @Now",

            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };

        var sql = $@"
            SELECT
                c.Id,
                c.CompanyId,
                c.EmployeeAccountId,
                c.Title,
                c.Description,
                c.EmploymentStartDate,
                c.EmploymentEndDate,
                c.Wage,
                c.CreatedAtUtc,
                a.FirstName AS EmployeeFirstName,
                a.LastName  AS EmployeeLastName
            FROM Contracts c
            JOIN Accounts a ON a.Id = c.EmployeeAccountId
            WHERE c.CompanyId = @CompanyId
              AND {statusCondition}
            ORDER BY c.CreatedAtUtc DESC;";

        await using var connection = dbContext.CreateConnection();
        await connection.OpenAsync(ct);

        return await DbCommandHelpers.ExecuteReaderAsync(
            connection,
            tx: null,
            sql: sql,
            ct: ct,
            configure: cmd =>
            {
                DbCommandHelpers.AddParam(cmd, "@CompanyId", DbType.Guid, companyId);
                DbCommandHelpers.AddParam(cmd, "@Now", DbType.Date, now.ToDateTime(TimeOnly.MinValue));
            },
            mapAsync: async (reader, token) =>
            {
                var results = new List<Contract>();

                var oId = reader.GetOrdinal("Id");
                var oCompanyId = reader.GetOrdinal("CompanyId");
                var oEmployeeAccountId = reader.GetOrdinal("EmployeeAccountId");
                var oTitle = reader.GetOrdinal("Title");
                var oDescription = reader.GetOrdinal("Description");
                var oStart = reader.GetOrdinal("EmploymentStartDate");
                var oEnd = reader.GetOrdinal("EmploymentEndDate");
                var oWage = reader.GetOrdinal("Wage");
                var oCreatedAt = reader.GetOrdinal("CreatedAtUtc");
                var oEmpFirst = reader.GetOrdinal("EmployeeFirstName");
                var oEmpLast = reader.GetOrdinal("EmployeeLastName");

                while (await reader.ReadAsync(token))
                {
                    results.Add(new Contract
                    {
                        Id = reader.GetGuid(oId),
                        CompanyId = reader.GetGuid(oCompanyId),
                        EmployeeAccountId = reader.GetGuid(oEmployeeAccountId),
                        Title = reader.GetString(oTitle),
                        Description = reader.IsDBNull(oDescription) ? null : reader.GetString(oDescription),

                        EmploymentStartDate =
                            DateOnly.FromDateTime(reader.GetDateTime(oStart)),

                        EmploymentEndDate = reader.IsDBNull(oEnd)
                            ? null
                            : DateOnly.FromDateTime(reader.GetDateTime(oEnd)),

                        Wage = reader.GetDecimal(oWage),
                        CreatedAtUtc = reader.GetDateTime(oCreatedAt),

                        EmployeeFirstName =
                            reader.IsDBNull(oEmpFirst) ? "" : reader.GetString(oEmpFirst),

                        EmployeeLastName =
                            reader.IsDBNull(oEmpLast) ? "" : reader.GetString(oEmpLast),
                    });
                }

                return results;
            });
    }



}


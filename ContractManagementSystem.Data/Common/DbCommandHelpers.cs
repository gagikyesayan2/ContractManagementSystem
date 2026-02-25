using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace ContractManagementSystem.Data.Common
{
    internal static class DbCommandHelpers
    {
        internal static async Task<int> ExecuteNonQueryAsync(DbConnection connection, DbTransaction tx, string sql, CancellationToken ct, Action<DbCommand> configure)
        {
            await using var cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            configure(cmd);

            return await cmd.ExecuteNonQueryAsync(ct);
        }

        internal static async Task<object?> ExecuteScalarAsync(DbConnection connection, DbTransaction tx, string sql, CancellationToken ct, Action<DbCommand> configure)
        {
            await using var cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            configure(cmd);

            return await cmd.ExecuteScalarAsync(ct);
        }

        internal static void AddParam(DbCommand command, string name, DbType type, object ?value, int? size = null)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = type;
            if (size.HasValue)
                parameter.Size = size.Value;

            parameter.Value = value ?? DBNull.Value;

            command.Parameters.Add(parameter);
        }

        internal static async Task<T> ExecuteReaderAsync<T>(DbConnection connection,DbTransaction tx,string sql,CancellationToken ct,Action<DbCommand> configure,Func<DbDataReader, CancellationToken, Task<T>> mapAsync)
        {
            await using var cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            configure(cmd);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await mapAsync(reader, ct);
        }
    }
}

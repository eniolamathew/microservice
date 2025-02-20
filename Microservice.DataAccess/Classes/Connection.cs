using LinqToDB.Data;
using MicroServices.DataAccess.Interfaces;
using Serilog;

namespace MicroServices.DataAccess.Classes
{
    public class Connection : DataConnection, IConnection
    {
        public Connection(string connectionString) : base("PostgreSQL", connectionString)
        {
            LinqToDB.Mapping.MappingSchema.Default.SetConverter<DateTime, DateTime>(x =>
            {
                if (x.Kind == DateTimeKind.Utc)
                {
                    return x.ToLocalTime();
                }
                if (x.Kind == DateTimeKind.Unspecified)
                {
                    return DateTime.SpecifyKind(x, DateTimeKind.Local);
                }
                return x;
            });
        }

        public async Task<DataConnectionTransaction> BeginTransactionAsync(System.Data.IsolationLevel level = System.Data.IsolationLevel.ReadCommitted)
        {
            return await base.BeginTransactionAsync(level);
        }

        public async Task CommitTransactionAsync(DataConnectionTransaction transaction)
        {
            await transaction.CommitAsync();
        }

        public async Task ExecuteQueryAsync(string query, int timeout = 120)
        {
            using var cmd = CreateCommand();
            try
            {
                cmd.CommandTimeout = timeout;
                cmd.CommandText = query;
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to execute query.");
                throw;
            }
            finally
            {
                await cmd.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<T?> ExecuteScalarAsync<T>(string query, int timeout = 120)
        {
            using var cmd = CreateCommand();
            try
            {
                cmd.CommandTimeout = timeout;
                cmd.CommandText = query;
                var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                return result == null || result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to execute scalar query.");
                throw;
            }
            finally
            {
                await cmd.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}

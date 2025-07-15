using LinqToDB.Data;
using MicroServices.DataAccess.Interfaces;
using Npgsql;
using Serilog;
using System.Data;

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

        public async Task ExecuteInTransactionAsync(Func<Task> repositoryMethod, IsolationLevel isolationLevel = IsolationLevel.Serializable, int maxRetries = 6, int timeOutRetries = 0)
        {
            var retryCount = 0;
            Random random = new Random();
            while (true)
            {
                try
                {
                    using (var tran = await BeginTransactionAsync(isolationLevel))
                    {
                        await repositoryMethod();
                        if (tran != null)
                        {
                            await tran.CommitAsync();
                        }
                    }
                    return;
                }
                catch (PostgresException e)
                {
                    if ((e is PostgresException exception && exception.SqlState == "40001"))
                    {
                        if (retryCount == maxRetries)
                        {
                            throw;
                        }
                        Thread.Sleep(random.Next(0, 101));
                        retryCount++;
                        continue;
                    }
                    if (e?.InnerException is TimeoutException)
                    {
                        if (retryCount == timeOutRetries)
                        {
                            Log.Error(e, "Command timed out. Giving up.");
                            throw;
                        }
                        Log.Error(e, "Command timed out. Retrying.");
                        retryCount++;
                        continue;
                    }
                    throw;
                }
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> repositoryMethod, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int maxRetries = 3)
        {
            var retryCount = 0;

            while (true)
            {
                try
                {
                    using (var tran = await BeginTransactionAsync(isolationLevel))
                    {
                        var result = await repositoryMethod();
                        if (tran != null)
                        {
                            await tran.CommitAsync();
                        }
                        return result;
                    }
                }
                catch (PostgresException e)
                {
                    if ((e is PostgresException exception && exception.SqlState == "40001"))
                    {
                        if (retryCount == maxRetries)
                        {
                            throw;
                        }
                        retryCount++;
                        continue;
                    }
                    throw;
                }
            }
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

        /// <summary>
        /// Performs a bulk insert using LinqToDB's BulkCopyAsync.
        /// </summary>
        public async Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
        {
            if (entities == null || !entities.Any())
                return;

            await this.BulkCopyAsync(new BulkCopyOptions { BulkCopyType = BulkCopyType.Default }, entities);
        }
    }
}

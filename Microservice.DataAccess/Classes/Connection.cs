using LinqToDB.Data;
using MicroServices.DataAccess.Interfaces;

namespace MicroServices.DataAccess.Classes
{
    public class Connection : DataConnection, IConnection
    {
        public Connection(string connectionString): base("PostgreSQL", connectionString)
        {
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
            var cmd = Connection.CreateCommand();
            cmd.CommandTimeout = timeout;
            cmd.CommandText = query;
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

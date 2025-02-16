using LinqToDB;
using LinqToDB.Data;


namespace MicroServices.DataAccess.Interfaces
{
    public interface IConnection : ICloneable, IDataContext
    {
        /// <summary>
        /// Starts a database transaction asynchronously.
        /// </summary>
        /// <param name="level">The isolation level of the transaction.</param>
        /// <returns>A transaction object.</returns>
        Task<DataConnectionTransaction> BeginTransactionAsync(System.Data.IsolationLevel level = System.Data.IsolationLevel.ReadCommitted);

        /// <summary>
        /// Commits a transaction asynchronously.
        /// </summary>
        /// <param name="transaction">The transaction to commit.</param>
        Task CommitTransactionAsync(DataConnectionTransaction transaction);

        /// <summary>
        /// Executes a raw SQL query asynchronously.
        /// </summary>
        /// <param name="query">The SQL query string.</param>
        /// <param name="timeout">Query timeout in seconds (default 55s).</param>
        Task ExecuteQueryAsync(string query, int timeout = 55);
    }
}

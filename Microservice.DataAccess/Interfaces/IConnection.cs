using LinqToDB;
using LinqToDB.Data;
using System.Data;
using System.Threading.Tasks;

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
        /// bulk insert logic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class;


        /// <summary>
        /// Executes a raw SQL query asynchronously.
        /// </summary>
        /// <param name="query">The SQL query string.</param>
        /// <param name="timeout">Query timeout in seconds (default 55s).</param>
        Task ExecuteQueryAsync(string query, int timeout = 55);

        /// <summary>
        /// Executes a raw SQL query that returns a scalar value asynchronously.
        /// </summary>
        /// <typeparam name="T">The expected return type of the scalar value.</typeparam>
        /// <param name="query">The SQL query string.</param>
        /// <param name="timeout">Query timeout in seconds (default 55s).</param>
        /// <returns>The scalar result of the query.</returns>
        Task<T?> ExecuteScalarAsync<T>(string query, int timeout = 55);


        // Add the method signature for transactions
        Task ExecuteInTransactionAsync(Func<Task> repositoryMethod, IsolationLevel isolationLevel = IsolationLevel.Serializable, int maxRetries = 6, int timeOutRetries = 0);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> repositoryMethod, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int maxRetries = 3);
    }
}

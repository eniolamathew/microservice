using Npgsql;
using Serilog;
using MicroServices.DataAccess.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseMigrationLib.Classes
{
    public class UpgradeDB
    {
        private readonly IConnection connection;
        private readonly string connectionString;
        private readonly string dbUserName;

        public UpgradeDB(IConnection connection, string connectionstring)
        {
            this.connection = connection;
            this.connectionString = connectionstring;
            dbUserName = connectionString; 

            Log.Information("Extracted Username from connection");

            // Ensure upgrade_history table exists
            if (!UpgradeHistoryTableExists().Result)
            {
                Log.Information("upgrade.upgrade_history table not found. Creating...");
                RunUpgradeScriptAsync("scripts/001_create_upgrade_history.sql").Wait();
            }
        }

        private string ExtractUserNameFromConnectionString(string connectionString)
        {
            // Assuming you extract username from the connection string
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return builder.Username ?? "default_user"; // fallback to a default user if not found
        }

        private async Task<bool> UpgradeHistoryTableExists()
        {
            const string checkTableSql = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables
                    WHERE table_schema = 'upgrade'
                    AND table_name = 'upgrade_history'
                );";

            var result = await connection.ExecuteScalarAsync<bool>(checkTableSql).ConfigureAwait(false);
            return result;
        }

        private async Task<int> GetDatabaseVersionAsync()
        {
            var getVersionSql = @"SELECT COALESCE(MAX(version), 0) FROM upgrade.upgrade_history";
            return await connection.ExecuteScalarAsync<int>(getVersionSql).ConfigureAwait(false);
        }

        private async Task RunUpgradeScriptAsync(string scriptPath)
        {
            try
            {
                if (!File.Exists(scriptPath))
                {
                    Log.Warning($"Script file not found: {scriptPath}");
                    return;
                }

                string script = await File.ReadAllTextAsync(scriptPath).ConfigureAwait(false);
                await connection.ExecuteQueryAsync(script).ConfigureAwait(false);
                Log.Information($"Executed script: {scriptPath}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to execute script: {scriptPath}");
            }
        }

        private async Task<bool> ExecuteSQLAsync(string sql)
        {
            try
            {
                await connection.ExecuteQueryAsync(sql).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Execute SQL error");
                return false;
            }
        }

        private async Task UpdateVersionAsync(int version, string fileName)
        {
            var updateVersionSql = $@"
                INSERT INTO upgrade.upgrade_history (upgraded_on, version, file_name)
                VALUES (CURRENT_TIMESTAMP, {version}, '{fileName}')";
            await connection.ExecuteQueryAsync(updateVersionSql).ConfigureAwait(false);
        }

        // One-time upgrade process
        private async Task<string> RunOnceAsync()
        {
            try
            {
                Log.Information("Starting database upgrade - Run Once");
                var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

                foreach (var file in Directory.GetFiles("./Scripts").OrderBy(a => a))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (int.TryParse(fileName.Substring(0, 5), out int fileVersion) && await IsNewAsync(fileName).ConfigureAwait(false))
                    {
                        var sql = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                        if (await ExecuteSQLAsync(sql).ConfigureAwait(false))
                        {
                            await UpdateVersionAsync(fileVersion, fileName).ConfigureAwait(false);
                        }
                    }
                }

                await connection.CommitTransactionAsync(transaction).ConfigureAwait(false);
                Log.Information("Upgrade completed - Run Once");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "RunOnceAsync failed");
                return ex.Message;
            }
        }

        private async Task<bool> IsNewAsync(string fileName)
        {
            var checkSql = $@"
                SELECT COUNT(1) FROM upgrade.upgrade_history 
                WHERE file_name = '{fileName}'";
            return await connection.ExecuteScalarAsync<int>(checkSql).ConfigureAwait(false) == 0;
        }

        // Mutable upgrade process
        private async Task<string> RunMutableAsync()
        {
            try
            {
                Log.Information("Starting database upgrade - Mutable");
                var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

                foreach (var file in Directory.GetFiles("./Mutable").OrderBy(a => a))
                {
                    var sql = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                    sql = sql.Replace("db_name_placeholder", Environment.GetEnvironmentVariable("DB_NAME"))
                             .Replace("TO upgrade", $"TO {dbUserName}");

                    if (!await ExecuteSQLAsync(sql).ConfigureAwait(false))
                    {
                        Log.Error($"Failed to execute SQL in {file}");
                        return $"Failed to execute SQL in {file}";
                    }
                }

                await connection.CommitTransactionAsync(transaction).ConfigureAwait(false);
                Log.Information("Upgrade completed - Mutable");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "RunMutableAsync failed");
                return ex.Message;
            }
        }

        // Main upgrade method
        public async Task<string> UpgradeAsync()
        {
            var results = await RunOnceAsync();

            if (!string.IsNullOrEmpty(results))
            {
                return results;
            }

            return await RunMutableAsync();
        }
    }
}
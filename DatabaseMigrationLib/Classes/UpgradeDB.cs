using Npgsql;
using Serilog;
using System.Transactions;
using MicroServices.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using DatabaseMigrationLib.Interface;

namespace DatabaseMigrationLib.Classes
{
    public class UpgradeDB : IUpgradeDB

    {
        private readonly IConnection _connection;
        private readonly string _scriptsPath;
        private readonly string _mutablePath;
        private readonly string _schemaName;
        private readonly string _historyTableName;
        private readonly string _upgradeRoleName;
        private readonly string _dbUserName;
        private readonly string _dbName;

        private UpgradeDB(IConnection connection, IConfiguration configuration, string serviceName)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            var connectionStringKey = $"{serviceName}_CONN";
            var connectionString = configuration[connectionStringKey]
                ?? throw new InvalidOperationException($"Missing {connectionStringKey} in configuration");

            Log.Information("Using connection string ({Key}): {ConnectionString}", connectionStringKey, connectionString);


            // Initialize all other fields synchronously
            _scriptsPath = configuration["DatabaseMigration:ScriptsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
            _mutablePath = configuration["DatabaseMigration:MutablePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Mutable");
            _schemaName = configuration["DatabaseMigration:SchemaName"] ?? "upgrade";
            _historyTableName = configuration["DatabaseMigration:HistoryTableName"] ?? "upgrade_history";
            _upgradeRoleName = configuration["DatabaseMigration:UpgradeRoleName"] ?? "upgrade";
            _dbName = configuration["DatabaseMigration:DatabaseName"] ?? new NpgsqlConnectionStringBuilder(connectionString).Database
                ?? throw new InvalidOperationException("Database name not specified");
            _dbUserName = new NpgsqlConnectionStringBuilder(connectionString).Username
                ?? throw new InvalidOperationException("Database username not specified in connection string");
        }

        public static async Task<UpgradeDB> CreateAsync(IConnection connection, IConfiguration configuration, string serviceName)
        {
            var instance = new UpgradeDB(connection, configuration, serviceName);
            await instance.InitializeDatabase();
            return instance;
        }

        private async Task InitializeDatabase()
        {
            if (!await UpgradeHistoryTableExists())
            {
                Log.Information("Creating upgrade history infrastructure...");
                await ExecuteScriptTransactionAsync(GetBaseSchemaScript());
            }
        }

        private string GetBaseSchemaScript() => $@"
            DO $$
            BEGIN
                -- Create upgrade role (for migration infrastructure)
                IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{_upgradeRoleName}') THEN
                    CREATE ROLE {_upgradeRoleName} WITH NOLOGIN;
                    COMMENT ON ROLE {_upgradeRoleName} IS 'Role for managing database migrations';
                END IF;
        
                -- Create platforms role (for application connections)
                IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{_dbUserName}') THEN
                    CREATE ROLE {_dbUserName} WITH LOGIN;
                    COMMENT ON ROLE {_dbUserName} IS 'Application role for platform services';
                END IF;
            END $$;

            CREATE SCHEMA IF NOT EXISTS {_schemaName} AUTHORIZATION {_upgradeRoleName};
    
            CREATE TABLE IF NOT EXISTS {_schemaName}.{_historyTableName} (
                id SERIAL PRIMARY KEY,
                upgraded_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                version INT NOT NULL,
                file_name TEXT NOT NULL,
                checksum TEXT NOT NULL,
                status TEXT NOT NULL,
                error_message TEXT,
                execution_time INTERVAL
            );

            -- Grant permissions to application user
            GRANT USAGE ON SCHEMA {_schemaName} TO {_dbUserName};
            GRANT SELECT, INSERT ON {_schemaName}.{_historyTableName} TO {_dbUserName};
            GRANT USAGE, SELECT ON SEQUENCE {_schemaName}.{_historyTableName}_id_seq TO {_dbUserName};";

        public async Task<string> UpgradeAsync()
        {
            try
            {
                var result = await RunVersionedMigrations();
                return string.IsNullOrEmpty(result)
                    ? await RunMutableMigrations()
                    : result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database upgrade failed");
                return $"Upgrade failed: {ex.Message}";
            }
        }

        private async Task<string> RunVersionedMigrations()
        {
            if (!Directory.Exists(_scriptsPath))
            {
                Log.Warning("Scripts directory not found at {Path}", _scriptsPath);
                return string.Empty;
            }

            var files = Directory.GetFiles(_scriptsPath, "*.sql")
                .OrderBy(f => Path.GetFileName(f))
                .ToList();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (await IsScriptAlreadyExecuted(fileName)) continue;

                try
                {
                    var script = await File.ReadAllTextAsync(file);
                    await ExecuteScriptTransactionAsync(script);

                    await RecordMigrationSuccess(
                        version: ExtractVersionNumber(fileName),
                        fileName: fileName,
                        checksum: CalculateChecksum(script)
                    );

                    Log.Information("Executed migration {FileName}", fileName);
                }
                catch (Exception ex)
                {
                    await RecordMigrationFailure(
                        version: ExtractVersionNumber(fileName),
                        fileName: fileName,
                        error: ex.Message
                    );
                    return $"Migration failed in {fileName}: {ex.Message}";
                }
            }
            return string.Empty;
        }

        private async Task<string> RunMutableMigrations()
        {
            if (!Directory.Exists(_mutablePath)) return string.Empty;

            var files = Directory.GetFiles(_mutablePath, "*.sql");
            foreach (var file in files)
            {
                try
                {
                    var script = (await File.ReadAllTextAsync(file))
                        .Replace("db_name_placeholder", _dbName)
                        .Replace("TO upgrade", $"TO {_dbUserName}");

                    await ExecuteScriptTransactionAsync(script);
                    Log.Information("Executed mutable script {FileName}", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    return $"Mutable script {Path.GetFileName(file)} failed: {ex.Message}";
                }
            }
            return string.Empty;
        }

        private async Task ExecuteScriptTransactionAsync(string script)
        {
            var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                .Where(b => !string.IsNullOrWhiteSpace(b));

            using var transaction = await _connection.BeginTransactionAsync();
            try
            {
                foreach (var batch in batches)
                {
                    await _connection.ExecuteQueryAsync(batch);
                }
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<bool> UpgradeHistoryTableExists()
        {
            var sql = $@"SELECT EXISTS (
                SELECT 1 FROM information_schema.tables 
                WHERE table_schema = '{_schemaName}' 
                AND table_name = '{_historyTableName}'
            )";
            return await _connection.ExecuteScalarAsync<bool>(sql);
        }

        private async Task<bool> IsScriptAlreadyExecuted(string fileName)
        {
            var sql = $@"
                SELECT COUNT(1) 
                FROM {_schemaName}.{_historyTableName} 
                WHERE file_name = '{fileName.Replace("'", "''")}'
                AND status = 'completed'";

            var count = await _connection.ExecuteScalarAsync<int>(sql);
            return count > 0;
        }

        private async Task RecordMigrationSuccess(int version, string fileName, string checksum)
        {
            var sql = $@"
                INSERT INTO {_schemaName}.{_historyTableName} 
                (version, file_name, checksum, status) 
                VALUES (
                    {version}, 
                    '{fileName.Replace("'", "''")}', 
                    '{checksum.Replace("'", "''")}',
                    'completed'
                )";

            await _connection.ExecuteQueryAsync(sql);
        }

        private async Task RecordMigrationFailure(int version, string fileName, string error)
        {
            var placeholderChecksum = "FAILED";

            var sql = $@"
            INSERT INTO {_schemaName}.{_historyTableName} 
            (version, file_name, checksum, status, error_message) 
            VALUES (
                {version}, 
                '{fileName.Replace("'", "''")}', 
                '{placeholderChecksum}',
                'failed', 
                '{error.Replace("'", "''")}'
            )";

            await _connection.ExecuteQueryAsync(sql);
        }


        private int ExtractVersionNumber(string fileName)
        {
            var match = Regex.Match(fileName, @"^(\d{3})");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private string CalculateChecksum(string content)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}


//public UpgradeDB(IConnection connection, IConfiguration configuration)
//{
//    _connection = connection ?? throw new ArgumentNullException(nameof(connection));

//    var connectionString = configuration.GetConnectionString("DefaultConnection")
//        ?? throw new InvalidOperationException("Missing DefaultConnection in configuration");

//    Log.Information("Using connection string: {ConnectionString}", connectionString);


//    // Get all configurable values
//    _scriptsPath = configuration["DatabaseMigration:ScriptsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
//    _mutablePath = configuration["DatabaseMigration:MutablePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Mutable");
//    _schemaName = configuration["DatabaseMigration:SchemaName"] ?? "upgrade";
//    _historyTableName = configuration["DatabaseMigration:HistoryTableName"] ?? "upgrade_history";
//    _upgradeRoleName = configuration["DatabaseMigration:UpgradeRoleName"] ?? "upgrade";
//    _dbName = configuration["DatabaseMigration:DatabaseName"] ?? new NpgsqlConnectionStringBuilder(connectionString).Database
//        ?? throw new InvalidOperationException("Database name not specified");
//    _dbUserName = new NpgsqlConnectionStringBuilder(connectionString).Username ?? throw new InvalidOperationException("Database username not specified in connection string");

//    InitializeDatabase().Wait();
//}

//private async Task InitializeDatabase()
//{
//    if (!await UpgradeHistoryTableExists())
//    {
//        Log.Information("Creating upgrade history infrastructure...");
//        await ExecuteScriptTransactionAsync(GetBaseSchemaScript());
//    }
//}
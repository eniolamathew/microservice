using DatabaseMigrationLib.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DatabaseMigrationLib.Classes
{
    public class UpgradeDBProxy : IDisposable, IUpgradeDBProxy
    {
        private readonly IUpgradeDBFactory _factory;
        private readonly IConfiguration _config;
        private readonly IConnectionFactory _connectionFactory;
        private bool _disposed;

        public UpgradeDBProxy(
            IUpgradeDBFactory factory,
            IConfiguration config,
            IConnectionFactory connectionFactory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IUpgradeDB> GetForServiceAsync(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));

            var connectionStringKey = $"{serviceName}_CONN";
            var connectionString = _config.GetConnectionString(connectionStringKey);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string '{connectionStringKey}' not found in configuration.");

            var scriptPath = Path.Combine(
                _config["DatabaseMigration:ScriptsPath"] ?? "/app/Scripts",
                serviceName);

            if (!Directory.Exists(scriptPath))
                throw new DirectoryNotFoundException($"Migration scripts directory not found: {scriptPath}");

            var serviceConfig = CreateServiceConfiguration(_config, serviceName, connectionString, scriptPath);

            var connection = _connectionFactory.Create(connectionString);
            return await UpgradeDB.CreateAsync(connection, serviceConfig, serviceName);
        }

        private IConfiguration CreateServiceConfiguration(IConfiguration rootConfig, string serviceName, string connectionString, string scriptPath)
        {
            var connectionStringKey = $"{serviceName}_CONN";

            var inMemoryCollection = new List<KeyValuePair<string, string?>>
            {
                new($"ConnectionStrings:{connectionStringKey}", connectionString),
                new("DatabaseMigration:ServiceName", serviceName),
                new("DatabaseMigration:ScriptsPath", scriptPath),
                new("DatabaseMigration:ConfigurationCreatedAt", DateTime.UtcNow.ToString("O"))
            };

            return new ConfigurationBuilder()
                .AddConfiguration(rootConfig)
                .AddInMemoryCollection(inMemoryCollection)
                .Build();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
        }

        ~UpgradeDBProxy() => Dispose(false);
    }
}





//using DatabaseMigrationLib.Interface;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Threading.Tasks;

//namespace DatabaseMigrationLib.Classes
//{
//    public class UpgradeDBProxy : IDisposable
//    {
//        private readonly Lazy<Task<IUpgradeDB>> _lazyUpgradeDb;
//        private readonly IUpgradeDBFactory _factory;
//        private readonly IConfiguration _config;
//        private readonly IConnectionFactory _connectionFactory;
//        private bool _disposed;

//        public UpgradeDBProxy(IUpgradeDBFactory factory, IConfiguration config, IConnectionFactory connectionFactory)
//        {
//            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
//            _config = config ?? throw new ArgumentNullException(nameof(config));
//            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

//            _lazyUpgradeDb = new Lazy<Task<IUpgradeDB>>(async () =>
//            {
//                var upgradeDb = await _factory.CreateAsync();
//                return (IUpgradeDB)upgradeDb;
//            });

//        }

//        public Task<IUpgradeDB> GetAsync() => _lazyUpgradeDb.Value;

//        public async Task<IUpgradeDB> GetForServiceAsync(string serviceName)
//        {
//            // 1. Validate input
//            if (string.IsNullOrWhiteSpace(serviceName))
//                throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));

//            // 2. Get service-specific connection string
//            var connectionStringKey = $"{serviceName}_CONN";
//            var connectionString = _config.GetConnectionString(connectionStringKey);

//            if (string.IsNullOrWhiteSpace(connectionString))
//                throw new InvalidOperationException($"Connection string '{connectionStringKey}' not found in configuration");

//            // 3. Set up script path (now using matching folder names)
//            var scriptPath = Path.Combine(_config["DatabaseMigration:ScriptsPath"] ?? "/app/Scripts", serviceName);

//            // Verify scripts directory exists
//            if (!Directory.Exists(scriptPath))
//                throw new DirectoryNotFoundException($"Migration scripts directory not found: {scriptPath}");

//            // 4. Create service-specific configuration
//            var serviceConfig = CreateServiceConfiguration(_config, serviceName, connectionString, scriptPath);

//            // 5. Create and return database instance
//            var connection = _connectionFactory.Create(connectionString);
//            var upgradeDbInstance = await UpgradeDB.CreateAsync(connection, serviceConfig, serviceName);

//            return (IUpgradeDB)upgradeDbInstance;
//        }

//        private IConfiguration CreateServiceConfiguration(IConfiguration rootConfig, string serviceName, string connectionString, string scriptPath)
//        {

//            var connectionStringKey = $"{serviceName}_CONN";

//            // Create the in-memory collection with nullable values
//            var inMemoryCollection = new List<KeyValuePair<string, string?>>
//            {
//                new KeyValuePair<string, string?>($"ConnectionStrings:{connectionStringKey}",connectionString),

//                new KeyValuePair<string, string?>("DatabaseMigration:ServiceName",serviceName),

//                new KeyValuePair<string, string?>("DatabaseMigration:ScriptsPath",scriptPath),

//                new KeyValuePair<string, string?>("DatabaseMigration:ConfigurationCreatedAt", DateTime.UtcNow.ToString("O"))
//            };

//            return new ConfigurationBuilder().AddConfiguration(rootConfig).AddInMemoryCollection(inMemoryCollection).Build();
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (_disposed) return;

//            if (disposing)
//            {
//                if (_lazyUpgradeDb.IsValueCreated)
//                {
//                    _lazyUpgradeDb.Value.ContinueWith(task =>
//                    {
//                        if (task.IsCompletedSuccessfully && task.Result is IDisposable disposable)
//                        {
//                            disposable.Dispose();
//                        }
//                    }).ConfigureAwait(false);
//                }
//            }

//            _disposed = true;
//        }

//        ~UpgradeDBProxy()
//        {
//            Dispose(false);
//        }
//    }
//}




//public async Task<IUpgradeDB> GetForServiceAsync(string serviceName)
//{
//    if (string.IsNullOrWhiteSpace(serviceName))
//        throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));

//    // 1. Get service-specific connection string
//    var connectionString = GetServiceConnectionString(serviceName);
//    if (string.IsNullOrWhiteSpace(connectionString))
//        throw new InvalidOperationException($"Connection string not found for service: {serviceName}");

//    // 2. Determine script folder (with your custom mapping)
//    var scriptSubfolder = GetScriptFolderForService(serviceName);
//    var scriptPath = Path.Combine(_config["DatabaseMigration:ScriptsPath"] ?? "/app/Scripts",scriptSubfolder);

//    // 3. Create enhanced configuration
//    var serviceConfig = CreateServiceConfiguration(_config, connectionString, scriptPath);

//    // 4. Create and return database instance
//    var connection = _connectionFactory.Create(connectionString);
//    var upgradeDbInstance = await UpgradeDB.CreateAsync(connection, serviceConfig);

//    return (IUpgradeDB)upgradeDbInstance;
//}

//public async Task<IUpgradeDB> GetForServiceAsync(string serviceName)
//{
//    if (string.IsNullOrWhiteSpace(serviceName))
//        throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));

//    var connectionString = GetServiceConnectionString(serviceName);
//    var scriptSubfolder = serviceName switch
//    {
//        "PlatformService" => "PlatformService",
//        "BrandService" => "BrandService",
//        _ => throw new ArgumentException("Invalid service name")
//    };

//    var scriptPath = Path.Combine(_config["DatabaseMigration:ScriptsPath"], scriptSubfolder);

//    var serviceConfig = CreateServiceConfiguration(_config, connectionString, scriptPath);

//    // Create connection for this service  
//    var connection = _connectionFactory.Create(connectionString);

//    // Use the static CreateAsync method to create an UpgradeDB instance  
//    var upgradeDbInstance = await UpgradeDB.CreateAsync(connection, serviceConfig);

//    // Explicitly cast UpgradeDB to IUpgradeDB
//    return (IUpgradeDB)upgradeDbInstance;
//}



//public async Task<IUpgradeDB> GetForServiceAsync(string serviceName)
//{
//    if (string.IsNullOrWhiteSpace(serviceName))
//        throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));

//    var connectionString = GetServiceConnectionString(serviceName);
//    if (string.IsNullOrWhiteSpace(connectionString))
//        throw new InvalidOperationException($"Connection string not found for service: {serviceName}");

//    // Create a new configuration for this specific service  
//    var serviceConfig = CreateServiceConfiguration(_config, connectionString);

//    // Create connection for this service  
//    var connection = _connectionFactory.Create(connectionString);

//    // Use the static CreateAsync method to create an UpgradeDB instance  
//    var upgradeDbInstance = await UpgradeDB.CreateAsync(connection, serviceConfig);

//    // Explicitly cast UpgradeDB to IUpgradeDB
//    return (IUpgradeDB)upgradeDbInstance;
//}

//private IConfiguration CreateServiceConfiguration(IConfiguration rootConfig, string connectionString)
//{
//    // Create a new configuration that combines the root config with service-specific settings
//    var configBuilder = new ConfigurationBuilder()
//        .AddConfiguration(rootConfig) // Base configuration
//        .AddInMemoryCollection(new[]
//        {
//            new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", connectionString)
//        });

//    return configBuilder.Build();
//}




//using DatabaseMigrationLib.Interface;
//using System;

//namespace DatabaseMigrationLib.Classes
//{
//    public class UpgradeDBProxy : IDisposable
//    {
//        private readonly Lazy<Task<UpgradeDB>> _lazyUpgradeDb;
//        private readonly IUpgradeDBFactory _factory;

//        public UpgradeDBProxy(IUpgradeDBFactory factory)
//        {
//            _factory = factory;
//            _lazyUpgradeDb = new Lazy<Task<UpgradeDB>>(() => _factory.CreateAsync());
//        }

//        public Task<UpgradeDB> GetAsync() => _lazyUpgradeDb.Value;

//        public async Task<IUpgradeDB> GetForServiceAsync(string serviceName)
//        {
//            // Get service-specific connection string
//            var connectionString = GetServiceConnectionString(serviceName);
//            return await _factory.CreateAsync(serviceName, connectionString);
//        }

//        private string GetServiceConnectionString(string serviceName)
//        {
//            return serviceName switch
//            {
//                "PlatformService" => _config["PLATFORM_DB_CONN"],
//                "BrandService" => _config["BRAND_DB_CONN"],
//                _ => throw new ArgumentException($"Unknown service: {serviceName}")
//            };
//        }

//        public void Dispose()
//        {
//            if (_lazyUpgradeDb.IsValueCreated)
//            {
//                _lazyUpgradeDb.Value.ContinueWith(task =>
//                {
//                    if (task.IsCompletedSuccessfully)
//                    {
//                        (task.Result as IDisposable)?.Dispose();

//                    }
//                });
//            }
//        }
//    }
//}

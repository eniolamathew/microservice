using DatabaseMigrationLib.Classes;
using DatabaseMigrationLib.Interface;
using MicroServices.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DatabaseMigrationLib.Factory
{
    public class UpgradeDBFactory : IUpgradeDBFactory
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;

        public UpgradeDBFactory(IConnectionFactory connectionFactory, IConfiguration configuration)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<UpgradeDB> CreateAsync(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name must be provided", nameof(serviceName));

            // Get service-specific connection string
            var connectionStringKey = $"{serviceName}_CONN";
            var connectionString = _configuration.GetConnectionString(connectionStringKey);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string '{connectionStringKey}' not found in configuration.");

            // Determine scripts path
            var scriptsBasePath = _configuration["DatabaseMigration:ScriptsPath"] ?? "/app/Scripts";
            var scriptPath = Path.Combine(scriptsBasePath, serviceName);

            if (!Directory.Exists(scriptPath))
                throw new DirectoryNotFoundException($"Migration scripts directory not found: {scriptPath}");

            // Create the connection using the factory
            var connection = _connectionFactory.Create(connectionString);

            // Create and return UpgradeDB instance
            var upgradeDb = await UpgradeDB.CreateAsync(connection, _configuration, serviceName);

            return upgradeDb;
        }
    }
}



//using DatabaseMigrationLib.Classes;
//using DatabaseMigrationLib.Interface;
//using MicroServices.DataAccess.Interfaces;
//using Microsoft.Extensions.Configuration;

//namespace DatabaseMigrationLib.Factory
//{
//    public class UpgradeDBFactory : IUpgradeDBFactory
//    {
//        private readonly IConnection _connection;
//        private readonly IConfiguration _configuration;

//        public UpgradeDBFactory(IConnection connection, IConfiguration configuration)
//        {
//            _connection = connection;
//            _configuration = configuration;
//        }

//        public async Task<UpgradeDB> CreateAsync(string serviceName)
//        {
//            return await UpgradeDB.CreateAsync(_connection, _configuration, serviceName);
//        }
//    }
//}



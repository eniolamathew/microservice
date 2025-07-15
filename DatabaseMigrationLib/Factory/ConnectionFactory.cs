using DatabaseMigrationLib.Interface;
using MicroServices.DataAccess.Classes;
using MicroServices.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;


namespace DatabaseMigrationLib.Factory
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly IConfiguration _config;

        public ConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public IConnection CreateForService(string serviceName)
        {
            // "If service is PlatformService, use PlatformService_CONN"
            var connectionString = _config.GetConnectionString($"{serviceName}_CONN");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string for service '{serviceName}' is null or empty.", nameof(serviceName));
            }
            return new Connection(connectionString);
        }

        public IConnection Create(string connectionString)
        {
            return new Connection(connectionString);
        }
    }
}

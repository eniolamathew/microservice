using MicroServices.DataAccess.Classes;
using MicroServices.DataAccess.Interfaces;

namespace DbMigrationRunner.Classes
{
    public class ServiceConnectionResolver
    {
        private readonly IConfiguration _config;

        public ServiceConnectionResolver(IConfiguration config)
        {
            _config = config;
        }

        public IConnection GetConnectionForService(string serviceName)
        {
            var connectionString = _config.GetConnectionString($"{serviceName}_CONN");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string for service '{serviceName}' is null or empty.", nameof(serviceName));
            }
            return new Connection(connectionString);
        }
    }
}

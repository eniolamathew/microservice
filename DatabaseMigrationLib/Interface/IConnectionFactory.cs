using MicroServices.DataAccess.Interfaces;

namespace DatabaseMigrationLib.Interface
{
    public interface IConnectionFactory
    {
       IConnection Create(string connectionString);
    }
}

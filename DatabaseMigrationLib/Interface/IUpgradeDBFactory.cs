using DatabaseMigrationLib.Classes;

namespace DatabaseMigrationLib.Interface
{
    public interface IUpgradeDBFactory
    {
        Task<UpgradeDB> CreateAsync(string serviceName);
    }
}
using System.Threading.Tasks;

namespace DatabaseMigrationLib.Interface
{
    public interface IUpgradeDB
    {
        Task<string?> UpgradeAsync(); 
    }
}

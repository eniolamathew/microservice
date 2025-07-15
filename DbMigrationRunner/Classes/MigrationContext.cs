using DbMigrationRunner.Interface;

namespace DbMigrationRunner.Classes
{
    public class MigrationContext : IMigrationContext
    {
        public required string CurrentService { get; set; }
    }
}

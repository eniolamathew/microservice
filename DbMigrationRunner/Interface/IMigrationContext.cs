namespace DbMigrationRunner.Interface
{
    public interface IMigrationContext
    {
        string CurrentService { get; set; }
    }
}

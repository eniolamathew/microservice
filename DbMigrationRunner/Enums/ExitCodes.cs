namespace DbMigrationRunner.Enums
{
    public enum ExitCodes
    {
        Success = 0,
        GeneralFailure = 1,
        Timeout = 2,
        ConnectionFailure = 3,
        MigrationFailure = 4,
        ConnectivityTestFailed = 5
    }
}
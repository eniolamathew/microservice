using MicroServices.DataAccess.Classes;
using MicroServices.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using SupplierService.Interfaces;
using SupplierService.Repo;
using Serilog;
using DatabaseMigrationLib.Classes;
using SupplierService.Classes;
using Polly;
using Npgsql;
using System.Net.Sockets;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.ServiceCaches;
using MicroServices.Caching.Extensions;
using SupplierService.Repo.Decorators;
using MicroServices.API.Clients;
using MicroServices.API.Interfaces;
using System.Net.Security;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register IConnection with the connection string from appsettings.json

builder.Services.AddScoped<IConnection>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();

    var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        // Build the connection string using environment variables
        connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = config["DATABASE_HOST"] ?? "localhost",
            Port = int.Parse(config["DATABASE_PORT"] ?? "5432"),
            Username = config["DATABASE_USER"],
            Password = config["DATABASE_PASSWORD"],
            Database = config["DATABASE_NAME"],
            SslMode = SslMode.Disable
        }.ToString();
    }

    return new Connection(connectionString);
});

// Register UpgradeDB as a scoped service to use IConnection and IConfiguration
//builder.Services.AddScoped<UpgradeDB>(provider =>
//{
//    var connection = provider.GetRequiredService<IConnection>();
//    var configuration = provider.GetRequiredService<IConfiguration>();
//    return new UpgradeDB(connection, configuration);
//});

builder.Services.AddLogging(configure => configure.AddSerilog());

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

// Add services to the container
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Injecting IConnection to PlatformDataProcessor
//builder.Services.AddScoped<ISupplierDataProcessor, SupplierDataProcessor>();

//builder.Services.AddScoped<ISupplierDomain, SupplierRepo>();

//builder.Services.Decorate<ISupplierDomain, CachedSupplierRepo>();

//builder.Services.AddScoped<IPlatformCache, PlatformCache>();

//builder.Services.AddAutoMapper(typeof(ConfigureMapping));

//builder.Services.AddMicroserviceCaching();

var supplierServiceUrl = builder.Configuration.GetValue<string>("Services:SupplierService");

// Capture the environment before building
//var isDevelopment = builder.Environment.IsDevelopment();

//builder.Services.AddHttpClient<IBrandApi, PlatformApi>(client =>
//{
//    client.BaseAddress = new Uri(supplierServiceUrl);
//})
//.ConfigurePrimaryHttpMessageHandler(() =>
//{
//    var handler = new HttpClientHandler();

//    if (isDevelopment)
//    {
//        handler.ServerCertificateCustomValidationCallback =
//            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
//    }

//    return handler;
//});

var app = builder.Build();

// Run DB Upgrade asynchronously with retry
await RunDatabaseUpgradeAsync(app.Services);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Async method to perform DB upgrade with retry logic using Polly
async Task RunDatabaseUpgradeAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var scopedServices = scope.ServiceProvider;
    var upgradeDb = scopedServices.GetRequiredService<UpgradeDB>();
    var config = scopedServices.GetRequiredService<IConfiguration>();
    var logger = scopedServices.GetRequiredService<ILogger<Program>>();

    // Configure retry policy with specific exception handling
    var retryPolicy = Policy
        .Handle<NpgsqlException>()
        .Or<SocketException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: attempt =>
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.LogWarning("Database connection attempt {Attempt} failed. Retrying in {DelaySeconds} seconds...",
                    attempt, delay.TotalSeconds);
                return delay;
            },
            onRetry: (exception, delay, attempt, context) =>
            {
                logger.LogWarning(exception, "Database upgrade attempt {Attempt} failed", attempt);
            });

    // Add a timeout policy (30 seconds max per attempt)
    var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(30));

    // Combine policies
    var policyWrap = Policy.WrapAsync(timeoutPolicy, retryPolicy);

    try
    {
        var success = await policyWrap.ExecuteAsync(async () =>
        {
            logger.LogInformation("Starting database upgrade...");

            var upgradeResult = await upgradeDb.UpgradeAsync();

            if (!string.IsNullOrEmpty(upgradeResult))
            {
                logger.LogError("Database upgrade failed with error: {UpgradeError}", upgradeResult);
                throw new Exception(upgradeResult); // Force retry
            }

            logger.LogInformation("Database upgrade completed successfully");
            return true;
        });

        if (!success)
        {
            logger.LogCritical("Database upgrade failed after all retry attempts");
            Environment.ExitCode = 1;
        }
    }
    catch (TimeoutException tex)
    {
        logger.LogCritical(tex, "Database upgrade timed out after all retry attempts");
        Environment.ExitCode = 2;
    }
    catch (NpgsqlException nex)
    {
        logger.LogCritical(nex, "Database connection failed after all retry attempts");
        Environment.ExitCode = 3;
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Critical failure during database upgrade");
        Environment.ExitCode = 4;
    }
    finally
    {
        if (Environment.ExitCode != 0)
        {
            // Allow time for logs to flush before exit
            await Task.Delay(500);
            Environment.Exit(Environment.ExitCode);
        }
    }
}

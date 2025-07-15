using DatabaseMigrationLib.Classes;
using DatabaseMigrationLib.Factory;
using DatabaseMigrationLib.Interface;
using DbMigrationRunner.Enums;
using Npgsql;
using Polly;
using Polly.Wrap;
using System.Net.Sockets;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddScoped<IConnectionFactory, ConnectionFactory>();
        services.AddScoped<IUpgradeDBFactory, UpgradeDBFactory>();
        services.AddScoped<UpgradeDBProxy>();

        services.AddSingleton(provider =>
            CreateResiliencePolicy(
                provider.GetRequiredService<ILogger<Program>>(),
                provider.GetRequiredService<IConfiguration>()
            )
        );
    });

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var config = host.Services.GetRequiredService<IConfiguration>();

try
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await RunDatabaseUpgradeAsync(host.Services);
    stopwatch.Stop();

    var completionMarkerPath = config.GetValue<string>("CompletionMarkerPath", "/tmp/migrations-complete");
    File.WriteAllText(completionMarkerPath, DateTime.UtcNow.ToString());

    logger.LogInformation("All migrations completed successfully in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
    return (int)ExitCodes.Success;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Migrations failed");
    return (int)ExitCodes.GeneralFailure;
}

AsyncPolicyWrap CreateResiliencePolicy(ILogger<Program> logger, IConfiguration config)
{
    var retryCount = config.GetValue<int>("MigrationSettings:RetryCount", 5);
    var timeoutSeconds = config.GetValue<int>("MigrationSettings:TimeoutSeconds", 30);

    var retryPolicy = Policy
        .Handle<NpgsqlException>()
        .Or<SocketException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            retryCount,
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            (ex, delay, attempt, ctx) =>
            {
                logger.LogWarning(ex, "Retry attempt {Attempt} failed, retrying in {Delay}s", attempt, delay.TotalSeconds);
            });

    var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(timeoutSeconds));
    return Policy.WrapAsync(timeoutPolicy, retryPolicy);
}

async Task RunDatabaseUpgradeAsync(IServiceProvider services)
{
    var config = services.GetRequiredService<IConfiguration>();
    var servicesToMigrate = config.GetSection("MigrationSettings:Services").Get<string[]>()
                           ?? new[] { "PlatformService", "BrandService" };

    foreach (var serviceName in servicesToMigrate)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;

        var logger = scopedServices.GetRequiredService<ILogger<Program>>();
        var policy = scopedServices.GetRequiredService<AsyncPolicyWrap>();
        var dbProxy = scopedServices.GetRequiredService<UpgradeDBProxy>();

        await MigrateServiceAsync(policy, dbProxy, logger, serviceName);
    }
}

async Task MigrateServiceAsync(AsyncPolicyWrap policy, UpgradeDBProxy dbProxy, ILogger logger, string serviceName)
{
    try
    {
        var success = await policy.ExecuteAsync(async () =>
        {
            logger.LogInformation("Starting database upgrade for {Service}...", serviceName);

            var upgradeDb = await dbProxy.GetForServiceAsync(serviceName);
            var result = await upgradeDb.UpgradeAsync();

            if (!string.IsNullOrEmpty(result))
            {
                logger.LogError("{Service} database upgrade failed: {Error}", serviceName, result);
                throw new Exception(result);
            }

            logger.LogInformation("{Service} database upgrade completed successfully", serviceName);
            return true;
        });

        if (!success)
        {
            logger.LogCritical("{Service} migration failed after all retries", serviceName);
            Environment.ExitCode = (int)ExitCodes.MigrationFailure;
        }
    }
    catch (TimeoutException tex)
    {
        logger.LogCritical(tex, "{Service} migration timed out", serviceName);
        Environment.ExitCode = (int)ExitCodes.Timeout;
    }
    catch (NpgsqlException nex)
    {
        logger.LogCritical(nex, "{Service} database connection failed", serviceName);
        Environment.ExitCode = (int)ExitCodes.ConnectionFailure;
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "{Service} critical failure during migration", serviceName);
        Environment.ExitCode = (int)ExitCodes.GeneralFailure;
    }
}





//using DatabaseMigrationLib.Classes;
//using DatabaseMigrationLib.Interface;
//using DatabaseMigrationLib.Factory;
//using DbMigrationRunner.Enums;
//using Microsoft.Extensions.Logging;
//using Npgsql;
//using Polly;
//using System.Net.Sockets;
//using Polly.Wrap;
//using DbMigrationRunner.Classes;
//using DbMigrationRunner.Interface;
//using MicroServices.DataAccess.Interfaces;

//var builder = Host.CreateDefaultBuilder(args)
//    .ConfigureLogging(logging =>
//    {
//        logging.ClearProviders();
//        logging.AddConsole();
//    })
//    .ConfigureServices((hostContext, services) =>
//    {
//        services.AddScoped<IMigrationContext, MigrationContext>();
//        services.AddTransient<ServiceConnectionResolver>();

//        services.AddScoped<IConnection>(provider =>
//        {
//            var context = provider.GetRequiredService<IMigrationContext>();
//            var resolver = provider.GetRequiredService<ServiceConnectionResolver>();
//            return resolver.GetConnectionForService(context.CurrentService);
//        });


//        services.AddTransient<IConnectionFactory, ConnectionFactory>();
//        services.AddTransient<IUpgradeDBFactory, UpgradeDBFactory>();
//        services.AddTransient<UpgradeDBProxy>();

//        services.AddSingleton(provider =>
//            CreateResiliencePolicy(
//                provider.GetRequiredService<ILogger<Program>>(),
//                provider.GetRequiredService<IConfiguration>()
//            )
//        );
//    });


//var host = builder.Build();
//var logger = host.Services.GetRequiredService<ILogger<Program>>();
//var config = host.Services.GetRequiredService<IConfiguration>();

//try
//{
//    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
//    await RunDatabaseUpgradeAsync(host.Services);
//    stopwatch.Stop();

//    // Create completion marker (configurable path)
//    var completionMarkerPath = config.GetValue<string>("CompletionMarkerPath", "/tmp/migrations-complete");
//    File.WriteAllText(completionMarkerPath, DateTime.UtcNow.ToString());

//    logger.LogInformation("All migrations completed successfully in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
//    return (int)ExitCodes.Success;
//}
//catch (Exception ex)
//{
//    logger.LogCritical(ex, "Migrations failed");
//    return (int)ExitCodes.GeneralFailure;
//}

//// Creates the combined resilience policy (retry + timeout)
//AsyncPolicyWrap CreateResiliencePolicy(ILogger<Program> logger, IConfiguration config)
//{
//    var retryCount = config.GetValue<int>("MigrationSettings:RetryCount", 5);
//    var timeoutSeconds = config.GetValue<int>("MigrationSettings:TimeoutSeconds", 30);

//    var retryPolicy = Policy
//        .Handle<NpgsqlException>()
//        .Or<SocketException>()
//        .Or<TimeoutException>()
//        .WaitAndRetryAsync(
//            retryCount: retryCount,
//            sleepDurationProvider: attempt =>
//            {
//                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
//                logger.LogWarning("Database connection attempt {Attempt} failed. Retrying in {DelaySeconds} seconds...",
//                    attempt, delay.TotalSeconds);
//                return delay;
//            },
//            onRetry: (exception, delay, attempt, context) =>
//            {
//                logger.LogWarning(exception, "Database upgrade attempt {Attempt} failed", attempt);
//            });

//    var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(timeoutSeconds));
//    return Policy.WrapAsync(timeoutPolicy, retryPolicy);
//}

//async Task RunDatabaseUpgradeAsync(IServiceProvider services)
//{
//    var config = services.GetRequiredService<IConfiguration>();
//    var servicesToMigrate = config.GetSection("MigrationSettings:Services").Get<string[]>()
//                           ?? new[] { "PlatformService", "BrandService" };

//    foreach (var serviceName in servicesToMigrate)
//    {
//        // New scope for each service
//        using var scope = services.CreateScope();
//        var scopedServices = scope.ServiceProvider;
//        var logger = scopedServices.GetRequiredService<ILogger<Program>>();
//        var policy = scopedServices.GetRequiredService<AsyncPolicyWrap>();

//        // Set current service in context
//        var context = scopedServices.GetRequiredService<IMigrationContext>();
//        context.CurrentService = serviceName;

//        var dbProxy = scopedServices.GetRequiredService<UpgradeDBProxy>();
//        await MigrateServiceAsync(policy, dbProxy, logger, serviceName);
//    }
//}

//// Handles migration for a single service
//async Task MigrateServiceAsync(AsyncPolicyWrap policy, UpgradeDBProxy dbProxy, ILogger<Program> logger, string serviceName)
//{
//    try
//    {
//        var success = await policy.ExecuteAsync(async () =>
//        {
//            logger.LogInformation("Starting database upgrade for {Service}...", serviceName);

//            var upgradeDb = await dbProxy.GetForServiceAsync(serviceName);
//            var upgradeResult = await upgradeDb.UpgradeAsync();

//            if (!string.IsNullOrEmpty(upgradeResult))
//            {
//                logger.LogError("{Service} database upgrade failed with error: {UpgradeError}",
//                    serviceName, upgradeResult);
//                throw new Exception(upgradeResult);
//            }

//            logger.LogInformation("{Service} database upgrade completed successfully", serviceName);
//            return true;
//        });

//        if (!success)
//        {
//            logger.LogCritical("{Service} database upgrade failed after all retry attempts", serviceName);
//            Environment.ExitCode = (int)ExitCodes.MigrationFailure;
//        }
//    }
//    catch (TimeoutException tex)
//    {
//        logger.LogCritical(tex, "{Service} database upgrade timed out after all retry attempts", serviceName);
//        Environment.ExitCode = (int)ExitCodes.Timeout;
//    }
//    catch (NpgsqlException nex)
//    {
//        logger.LogCritical(nex, "{Service} database connection failed after all retry attempts", serviceName);
//        Environment.ExitCode = (int)ExitCodes.ConnectionFailure;
//    }
//    catch (Exception ex)
//    {
//        logger.LogCritical(ex, "{Service} critical failure during database upgrade", serviceName);
//        Environment.ExitCode = (int)ExitCodes.GeneralFailure;
//    }
//}







//using DatabaseMigrationLib.Classes;
//using DatabaseMigrationLib.Interface;
//using DatabaseMigrationLib.Factory;
//using Microsoft.Extensions.Logging;
//using Npgsql;
//using Polly;
//using System.Net.Sockets;

//var builder = Host.CreateDefaultBuilder(args)
//    .ConfigureLogging(logging =>
//    {
//        logging.ClearProviders();
//        logging.AddConsole();
//    })
//    .ConfigureServices((hostContext, services) =>
//    {
//        services.AddTransient<UpgradeDB>();
//        services.AddTransient<IUpgradeDBFactory, UpgradeDBFactory>();
//    });

//var host = builder.Build();
//var logger = host.Services.GetRequiredService<ILogger<Program>>();

//try
//{
//    await RunDatabaseUpgradeAsync(host.Services);

//    // Create completion marker
//    File.WriteAllText("/tmp/migrations-complete", DateTime.UtcNow.ToString());

//    logger.LogInformation("All migrations completed successfully");
//    return 0;
//}
//catch (Exception ex)
//{
//    logger.LogCritical(ex, "Migrations failed");
//    return 1;
//}

//// Async method to perform DB upgrade with retry logic using Polly
//async Task RunDatabaseUpgradeAsync(IServiceProvider services)
//{
//    using var scope = services.CreateScope();
//    var scopedServices = scope.ServiceProvider;
//    var dbProxy = scopedServices.GetRequiredService<UpgradeDBProxy>();
//    var config = scopedServices.GetRequiredService<IConfiguration>();
//    var logger = scopedServices.GetRequiredService<ILogger<Program>>();

//    // Configure retry policy with specific exception handling
//    var retryPolicy = Policy
//        .Handle<NpgsqlException>()
//        .Or<SocketException>()
//        .Or<TimeoutException>()
//        .WaitAndRetryAsync(
//            retryCount: 5,
//            sleepDurationProvider: attempt =>
//            {
//                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
//                logger.LogWarning("Database connection attempt {Attempt} failed. Retrying in {DelaySeconds} seconds...",
//                    attempt, delay.TotalSeconds);
//                return delay;
//            },
//            onRetry: (exception, delay, attempt, context) =>
//            {
//                logger.LogWarning(exception, "Database upgrade attempt {Attempt} failed", attempt);
//            });

//    // Add a timeout policy (30 seconds max per attempt)
//    var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(30));

//    // Combine policies
//    var policyWrap = Policy.WrapAsync(timeoutPolicy, retryPolicy);

//    try
//    {
//        // Define the services that need migrations
//        var servicesToMigrate = new[] { "PlatformService", "BrandService" };

//        foreach (var serviceName in servicesToMigrate)
//        {
//            var success = await policyWrap.ExecuteAsync(async () =>
//            {
//                logger.LogInformation("Starting database upgrade for {Service}...", serviceName);

//                // Configure the proxy for the current service
//                var upgradeDb = await dbProxy.GetForServiceAsync(serviceName);

//                var upgradeResult = await upgradeDb.UpgradeAsync();

//                if (!string.IsNullOrEmpty(upgradeResult))
//                {
//                    logger.LogError("{Service} database upgrade failed with error: {UpgradeError}",
//                        serviceName, upgradeResult);
//                    throw new Exception(upgradeResult); // Force retry
//                }

//                logger.LogInformation("{Service} database upgrade completed successfully", serviceName);
//                return true;
//            });

//            if (!success)
//            {
//                logger.LogCritical("{Service} database upgrade failed after all retry attempts", serviceName);
//                Environment.ExitCode = 1;
//                return;
//            }
//        }
//    }
//    catch (TimeoutException tex)
//    {
//        logger.LogCritical(tex, "Database upgrade timed out after all retry attempts");
//        Environment.ExitCode = 2;
//    }
//    catch (NpgsqlException nex)
//    {
//        logger.LogCritical(nex, "Database connection failed after all retry attempts");
//        Environment.ExitCode = 3;
//    }
//    catch (Exception ex)
//    {
//        logger.LogCritical(ex, "Critical failure during database upgrade");
//        Environment.ExitCode = 4;
//    }
//    finally
//    {
//        if (Environment.ExitCode != 0)
//        {
//            // Allow time for logs to flush before exit
//            await Task.Delay(500);
//            Environment.Exit(Environment.ExitCode);
//        }
//    }
//}




//using MicroServices.DataAccess.Classes;
//using MicroServices.DataAccess.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using PlatformService.Interfaces;
//using PlatformService.Repo;
//using Serilog;
//using PlatformService.Classes;
//using Polly;
//using Npgsql;
//using System.Net.Sockets;
//using MicroServices.Caching.Interfaces;
//using MicroServices.Caching.ServiceCaches;
//using MicroServices.Caching.Extensions;
//using MicroServices.API.Clients;
//using MicroServices.API.Interfaces;
//using System.Net.Security;
//using Microsoft.Extensions.DependencyInjection;
//using DatabaseMigrationLib.Classes;
//using PlatformService.Repo.Decorators;
//using DatabaseMigrationLib.Interface;
//using DatabaseMigrationLib.Factory;

//var builder = WebApplication.CreateBuilder(args);

//// Register IConnection with the connection string from appsettings.json

//builder.Services.AddScoped<IConnection>(provider =>
//{
//    var config = provider.GetRequiredService<IConfiguration>();

//    var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");

//    if (string.IsNullOrEmpty(connectionString))
//    {
//        // Build the connection string using environment variables
//        connectionString = new NpgsqlConnectionStringBuilder
//        {
//            Host = config["DATABASE_HOST"] ?? "localhost",
//            Port = int.Parse(config["DATABASE_PORT"] ?? "5432"),
//            Username = config["DATABASE_USER"],
//            Password = config["DATABASE_PASSWORD"],
//            Database = config["DATABASE_NAME"],
//            SslMode = SslMode.Disable
//        }.ToString();
//    }

//    return new Connection(connectionString);
//});

////Register UpgradeDB as a scoped service to use IConnection and IConfiguration
////builder.Services.AddScoped<UpgradeDB>(async provider =>
////{
////    var connection = provider.GetRequiredService<IConnection>();
////var configuration = provider.GetRequiredService<IConfiguration>();
////return await UpgradeDB.CreateAsync(connection, configuration);
////    //return new UpgradeDB(connection, configuration);
////});


//// Register UpgradeDB as a scoped service using the factory
//builder.Services.AddScoped<IUpgradeDBFactory, UpgradeDBFactory>();
//builder.Services.AddScoped<UpgradeDBProxy>();

//builder.Services.AddLogging(configure => configure.AddSerilog());

//// Configure Serilog
//Log.Logger = new LoggerConfiguration()
//    .WriteTo.Console()
//    .CreateLogger();

//// Add services to the container
//builder.Services.AddControllers();
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Run DB Upgrade asynchronously with retry
//await RunDatabaseUpgradeAsync(app.Services);

//// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();

//// Async method to perform DB upgrade with retry logic using Polly
//async Task RunDatabaseUpgradeAsync(IServiceProvider services)
//{
//    using var scope = services.CreateScope();
//    var scopedServices = scope.ServiceProvider;
//    var dbProxy = scopedServices.GetRequiredService<UpgradeDBProxy>();
//    var config = scopedServices.GetRequiredService<IConfiguration>();
//    var logger = scopedServices.GetRequiredService<ILogger<Program>>();

//    // Configure retry policy with specific exception handling
//    var retryPolicy = Policy
//        .Handle<NpgsqlException>()
//        .Or<SocketException>()
//        .Or<TimeoutException>()
//        .WaitAndRetryAsync(
//            retryCount: 5,
//            sleepDurationProvider: attempt =>
//            {
//                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
//                logger.LogWarning("Database connection attempt {Attempt} failed. Retrying in {DelaySeconds} seconds...",
//                    attempt, delay.TotalSeconds);
//                return delay;
//            },
//            onRetry: (exception, delay, attempt, context) =>
//            {
//                logger.LogWarning(exception, "Database upgrade attempt {Attempt} failed", attempt);
//            });

//    // Add a timeout policy (30 seconds max per attempt)
//    var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(30));

//    // Combine policies
//    var policyWrap = Policy.WrapAsync(timeoutPolicy, retryPolicy);

//    try
//    {
//        var success = await policyWrap.ExecuteAsync(async () =>
//        {
//            logger.LogInformation("Starting database upgrade...");
//            var upgradeDb = await dbProxy.GetAsync();

//            var upgradeResult = await upgradeDb.UpgradeAsync();

//            if (!string.IsNullOrEmpty(upgradeResult))
//            {
//                logger.LogError("Database upgrade failed with error: {UpgradeError}", upgradeResult);
//                throw new Exception(upgradeResult); // Force retry
//            }

//            logger.LogInformation("Database upgrade completed successfully");
//            return true;
//        });

//        if (!success)
//        {
//            logger.LogCritical("Database upgrade failed after all retry attempts");
//            Environment.ExitCode = 1;
//        }
//    }
//    catch (TimeoutException tex)
//    {
//        logger.LogCritical(tex, "Database upgrade timed out after all retry attempts");
//        Environment.ExitCode = 2;
//    }
//    catch (NpgsqlException nex)
//    {
//        logger.LogCritical(nex, "Database connection failed after all retry attempts");
//        Environment.ExitCode = 3;
//    }
//    catch (Exception ex)
//    {
//        logger.LogCritical(ex, "Critical failure during database upgrade");
//        Environment.ExitCode = 4;
//    }
//    finally
//    {
//        if (Environment.ExitCode != 0)
//        {
//            // Allow time for logs to flush before exit
//            await Task.Delay(500);
//            Environment.Exit(Environment.ExitCode);
//        }
//    }
//}



//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using MicroServices.DataAccess.Classes;
using MicroServices.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using PlatformService.Interfaces;
using PlatformService.Repo;
using Serilog;
using DatabaseMigrationLib.Classes;

var builder = WebApplication.CreateBuilder(args);

// Read the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("The connection string 'DefaultConnection' is missing or empty in appsettings.json.");
}

// Register IConnection with the connection string from appsettings.json
builder.Services.AddScoped<IConnection>(provider => new Connection(connectionString));

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register UpgradeDB as a scoped service to use IConnection
builder.Services.AddScoped<UpgradeDB>();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMemory"));
builder.Services.AddScoped<IPlatform, PlatformRepo>();

var app = builder.Build();

// Run DB Upgrade asynchronously
await RunDatabaseUpgradeAsync(app.Services, connectionString);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Async method to perform DB upgrade

async Task RunDatabaseUpgradeAsync(IServiceProvider services, string connectionString)
{
    var connection = services.GetRequiredService<IConnection>();
    var upgradeDb = new UpgradeDB(connection, connectionString); 
    var upgradeResult = await upgradeDb.UpgradeAsync();

    if (!string.IsNullOrEmpty(upgradeResult))
    {
        Log.Fatal("Database upgrade failed: {Error}", upgradeResult);
        Environment.Exit(1); // Exit if upgrade fails
    }

    Log.Information("Database upgrade completed successfully.");
}


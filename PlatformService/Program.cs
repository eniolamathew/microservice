using MicroServices.DataAccess.Classes;
using MicroServices.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using PlatformService.Interfaces;
using PlatformService.Repo;

var builder = WebApplication.CreateBuilder(args);

// Read the connection string from appsettings.json

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("The connection string 'Connection' is missing or empty in appsettings.json.");
}

// Register IBackOfficeConnection with the connection string from appsettings.json

builder.Services.AddScoped<IConnection>(provider => new Connection(connectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMemory"));
builder.Services.AddScoped<IPlatform, PlatformRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

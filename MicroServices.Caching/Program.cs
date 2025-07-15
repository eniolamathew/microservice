using MicroServices.Caching.Extensions;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Register caching services
builder.Services.AddMicroserviceCaching();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
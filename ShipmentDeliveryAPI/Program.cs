using Microsoft.EntityFrameworkCore;
using ShipmentDeliveryAPI.Data;
using ShipmentDeliveryAPI.Repositories;
using ShipmentDeliveryAPI.Repositories.Interfaces;
using ShipmentDeliveryAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Entity Framework - configured to work with both SQLite (dev) and SQL Server (production)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString!.Contains("Data Source=") && connectionString.Contains(".db"))
{
    // SQLite configuration for development
    builder.Services.AddDbContext<ShipmentDeliveryContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    // SQL Server configuration for production/Azure
    builder.Services.AddDbContext<ShipmentDeliveryContext>(options =>
        options.UseSqlServer(connectionString));
}

// Register repositories
builder.Services.AddScoped<IGenericRepository<object>, GenericRepository<object>>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IContainerItemRepository, ContainerItemRepository>();
builder.Services.AddScoped<IBulkItemRepository, BulkItemRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<IShipmentDeliveryService, ShipmentDeliveryService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Shipment Delivery API",
        Version = "v1",
        Description = "API for managing shipment and delivery data with container and bulk items using Repository and Unit of Work patterns"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

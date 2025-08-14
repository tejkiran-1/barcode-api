using Microsoft.EntityFrameworkCore;
using ShipmentDeliveryAPI.Data;
using ShipmentDeliveryAPI.Repositories;
using ShipmentDeliveryAPI.Repositories.Interfaces;
using ShipmentDeliveryAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Entity Framework - configured to work with SQL Server/Azure SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ShipmentDeliveryContext>(options =>
    options.UseSqlServer(connectionString));

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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular default ports
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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

// Enable CORS
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

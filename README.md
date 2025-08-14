# Shipment Delivery API

A .NET Core Web API for managing shipment and delivery data with support for container and bulk delivery types, built using **Repository Pattern**, **Unit of Work Pattern**, and **Clean Architecture** principles.

## Architecture Overview

This API follows industry-standard patterns:
- **Controller-Service-Repository Pattern**: Separation of concerns with clear layers
- **Unit of Work Pattern**: Centralized transaction management
- **Dependency Injection**: Loose coupling and testability
- **Generic Repository**: Code reusability and consistency
- **Entity Framework Core**: ORM with Code First approach

## Project Structure

```
ShipmentDeliveryAPI/
├── Controllers/           # API Controllers
├── Services/             # Business Logic Layer
├── Repositories/         # Data Access Layer
│   ├── Interfaces/      # Repository Contracts
│   └── Implementations/ # Repository Implementations
├── Data/                # DbContext and Migrations
├── Models/              # Domain Entities
├── DTOs/                # Data Transfer Objects
└── Migrations/          # EF Core Migrations
```

## Data Model

### Shipment
- Can have multiple deliveries
- Identified by unique shipment number

### Delivery
- Belongs to one shipment
- Can be either Container or Bulk type
- Identified by unique delivery number

### Container Items (for Container deliveries)
- Material Number + Serial Number pairs

### Bulk Items (for Bulk deliveries)
- Material Number + EVD Seal Number pairs

## API Endpoints

### 1. Create Shipment Delivery
**POST** `/api/shipmentdelivery`

#### Container Delivery Example:
```json
{
  "shipmentNumber": "SH001",
  "deliveryNumber": "DL001", 
  "deliveryType": 0,
  "containerItems": [
    {
      "materialNumber": "MAT001",
      "serialNumber": "SER001"
    }
  ]
}
```

#### Bulk Delivery Example:
```json
{
  "shipmentNumber": "SH001",
  "deliveryNumber": "DL002",
  "deliveryType": 1,
  "bulkItems": [
    {
      "materialNumber": "MAT003",
      "evdSealNumber": "EVD001"
    }
  ]
}
```

**Delivery Types:**
- 0 = Container
- 1 = Bulk

### 2. Get Shipment by Shipment Number
**GET** `/api/shipmentdelivery/shipment/{shipmentNumber}`

Returns all deliveries associated with the shipment number.

### 3. Get Shipment by Delivery Number  
**GET** `/api/shipmentdelivery/delivery/{deliveryNumber}`

Returns the shipment and all its deliveries based on a specific delivery number.

### 4. Get All Shipments
**GET** `/api/shipmentdelivery`

Returns all shipments with their deliveries.

## Database Support

The API supports both **SQLite** (for development) and **SQL Server** (for production/Azure):

### Development (SQLite)
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=shipmentdelivery.db"
}
```

### Production/Azure (SQL Server)
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-azure-sql-server.database.windows.net;Database=ShipmentDeliveryDB;User ID=your-username;Password=your-password;Trusted_Connection=false;MultipleActiveResultSets=true;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"
}
```

The application automatically detects the connection string format and uses the appropriate provider.

## Repository Pattern Implementation

### Generic Repository
Provides common CRUD operations for all entities:
```csharp
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    // ... more methods
}
```

### Specific Repositories
Each entity has its own repository with specialized methods:
- `IShipmentRepository` - Shipment-specific queries
- `IDeliveryRepository` - Delivery-specific queries
- `IContainerItemRepository` - Container item operations
- `IBulkItemRepository` - Bulk item operations

### Unit of Work
Manages transactions and provides access to all repositories:
```csharp
public interface IUnitOfWork : IDisposable
{
    IShipmentRepository Shipments { get; }
    IDeliveryRepository Deliveries { get; }
    IContainerItemRepository ContainerItems { get; }
    IBulkItemRepository BulkItems { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## Setup and Running

### Prerequisites
- .NET 8.0 SDK
- SQL Server (for production) or SQLite (for development)

### Steps to Run

1. **Clone and navigate to the project:**
   ```bash
   cd ShipmentDeliveryAPI
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Update database connection string** in `appsettings.json` if needed.

4. **Create and apply database migrations:**
   ```bash
   dotnet ef database update
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI:**
   Navigate to `https://localhost:5001/swagger` (or the port shown in console)

## Migration to Azure SQL Server

To migrate from SQLite to Azure SQL Server:

1. **Update Connection String:**
   Replace the `DefaultConnection` in `appsettings.json` or `appsettings.Production.json`

2. **The application will automatically detect SQL Server and use the appropriate provider**

3. **Run migrations:**
   ```bash
   dotnet ef database update
   ```

## Database Schema

The API uses Entity Framework Core with the following tables:
- **Shipments** - Main shipment records
- **Deliveries** - Delivery records linked to shipments
- **ContainerItems** - Items for container-type deliveries
- **BulkItems** - Items for bulk-type deliveries

## Features

- ✅ **Repository Pattern** - Clean data access layer
- ✅ **Unit of Work Pattern** - Transaction management
- ✅ **Generic Repository** - Code reusability
- ✅ **Dependency Injection** - Loose coupling
- ✅ **Database Provider Flexibility** - SQLite/SQL Server support
- ✅ **Unique constraint validation** - Shipment and delivery numbers
- ✅ **Automatic shipment creation** - If it doesn't exist
- ✅ **Transaction support** - Data consistency
- ✅ **Comprehensive error handling** - With proper logging
- ✅ **Swagger/OpenAPI documentation** - API documentation
- ✅ **Proper HTTP status codes** - RESTful responses
- ✅ **Logging support** - Application insights

## Benefits of This Architecture

1. **Maintainability**: Clear separation of concerns
2. **Testability**: Easy to unit test with mocked repositories
3. **Scalability**: Easy to add new features and entities
4. **Flexibility**: Can switch database providers easily
5. **Consistency**: Standardized data access patterns
6. **Transaction Safety**: Proper rollback mechanisms

## Testing

Use the provided `ShipmentDeliveryAPI.http` file with VS Code REST Client extension, or use Swagger UI for testing the endpoints.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShipmentDeliveryAPI.Data;
using ShipmentDeliveryAPI.Repositories.Interfaces;

namespace ShipmentDeliveryAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ShipmentDeliveryContext _context;
        private IDbContextTransaction? _transaction;

        private IShipmentRepository? _shipments;
        private IDeliveryRepository? _deliveries;
        private IContainerItemRepository? _containerItems;
        private IBulkItemRepository? _bulkItems;

        public UnitOfWork(ShipmentDeliveryContext context)
        {
            _context = context;
        }

        public IShipmentRepository Shipments =>
            _shipments ??= new ShipmentRepository(_context);

        public IDeliveryRepository Deliveries =>
            _deliveries ??= new DeliveryRepository(_context);

        public IContainerItemRepository ContainerItems =>
            _containerItems ??= new ContainerItemRepository(_context);

        public IBulkItemRepository BulkItems =>
            _bulkItems ??= new BulkItemRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}

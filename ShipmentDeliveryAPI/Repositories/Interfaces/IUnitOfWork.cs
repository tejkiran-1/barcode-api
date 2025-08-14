namespace ShipmentDeliveryAPI.Repositories.Interfaces
{
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
}

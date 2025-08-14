using ShipmentDeliveryAPI.Models;

namespace ShipmentDeliveryAPI.Repositories.Interfaces
{
    public interface IBulkItemRepository : IGenericRepository<BulkItem>
    {
        Task<IEnumerable<BulkItem>> GetByDeliveryIdAsync(int deliveryId);
        Task<bool> BulkItemExistsAsync(int deliveryId, string materialNumber, string evdSealNumber);
    }
}

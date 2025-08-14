using Microsoft.EntityFrameworkCore;
using ShipmentDeliveryAPI.Data;
using ShipmentDeliveryAPI.Models;
using ShipmentDeliveryAPI.Repositories.Interfaces;

namespace ShipmentDeliveryAPI.Repositories
{
    public class BulkItemRepository : GenericRepository<BulkItem>, IBulkItemRepository
    {
        public BulkItemRepository(ShipmentDeliveryContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BulkItem>> GetByDeliveryIdAsync(int deliveryId)
        {
            return await _dbSet.Where(bi => bi.DeliveryId == deliveryId).ToListAsync();
        }

        public async Task<bool> BulkItemExistsAsync(int deliveryId, string materialNumber, string evdSealNumber)
        {
            return await _dbSet.AnyAsync(bi =>
                bi.DeliveryId == deliveryId &&
                bi.MaterialNumber == materialNumber &&
                bi.EvdSealNumber == evdSealNumber);
        }
    }
}

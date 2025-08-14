using Microsoft.EntityFrameworkCore;
using ShipmentDeliveryAPI.Data;
using ShipmentDeliveryAPI.Models;
using ShipmentDeliveryAPI.Repositories.Interfaces;

namespace ShipmentDeliveryAPI.Repositories
{
    public class ContainerItemRepository : GenericRepository<ContainerItem>, IContainerItemRepository
    {
        public ContainerItemRepository(ShipmentDeliveryContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ContainerItem>> GetByDeliveryIdAsync(int deliveryId)
        {
            return await _dbSet.Where(ci => ci.DeliveryId == deliveryId).ToListAsync();
        }

        public async Task<bool> ContainerItemExistsAsync(int deliveryId, string materialNumber, string serialNumber)
        {
            return await _dbSet.AnyAsync(ci =>
                ci.DeliveryId == deliveryId &&
                ci.MaterialNumber == materialNumber &&
                ci.SerialNumber == serialNumber);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using ShipmentDeliveryAPI.Data;
using ShipmentDeliveryAPI.Models;
using ShipmentDeliveryAPI.Repositories.Interfaces;

namespace ShipmentDeliveryAPI.Repositories
{
    public class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
    {
        public DeliveryRepository(ShipmentDeliveryContext context) : base(context)
        {
        }

        public async Task<Delivery?> GetByDeliveryNumberAsync(string deliveryNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.DeliveryNumber == deliveryNumber);
        }

        public async Task<Delivery?> GetDeliveryWithItemsAsync(string deliveryNumber)
        {
            return await _dbSet
                .Include(d => d.ContainerItems)
                .Include(d => d.BulkItems)
                .FirstOrDefaultAsync(d => d.DeliveryNumber == deliveryNumber);
        }

        public async Task<Delivery?> GetDeliveryWithShipmentAsync(string deliveryNumber)
        {
            return await _dbSet
                .Include(d => d.Shipment)
                    .ThenInclude(s => s.Deliveries)
                        .ThenInclude(d => d.ContainerItems)
                .Include(d => d.Shipment)
                    .ThenInclude(s => s.Deliveries)
                        .ThenInclude(d => d.BulkItems)
                .FirstOrDefaultAsync(d => d.DeliveryNumber == deliveryNumber);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByShipmentIdAsync(int shipmentId)
        {
            return await _dbSet
                .Include(d => d.ContainerItems)
                .Include(d => d.BulkItems)
                .Where(d => d.ShipmentId == shipmentId)
                .ToListAsync();
        }

        public async Task<bool> DeliveryExistsAsync(string deliveryNumber)
        {
            return await _dbSet.AnyAsync(d => d.DeliveryNumber == deliveryNumber);
        }
    }
}

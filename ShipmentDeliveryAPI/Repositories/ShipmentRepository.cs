using Microsoft.EntityFrameworkCore;
using ShipmentDeliveryAPI.Data;
using ShipmentDeliveryAPI.Models;
using ShipmentDeliveryAPI.Repositories.Interfaces;

namespace ShipmentDeliveryAPI.Repositories
{
    public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(ShipmentDeliveryContext context) : base(context)
        {
        }

        public async Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.ShipmentNumber == shipmentNumber);
        }

        public async Task<Shipment?> GetShipmentWithDeliveriesAsync(string shipmentNumber)
        {
            return await _dbSet
                .Include(s => s.Deliveries)
                    .ThenInclude(d => d.ContainerItems)
                .Include(s => s.Deliveries)
                    .ThenInclude(d => d.BulkItems)
                .FirstOrDefaultAsync(s => s.ShipmentNumber == shipmentNumber);
        }

        public async Task<bool> ShipmentExistsAsync(string shipmentNumber)
        {
            return await _dbSet.AnyAsync(s => s.ShipmentNumber == shipmentNumber);
        }
    }
}

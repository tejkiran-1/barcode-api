using ShipmentDeliveryAPI.Models;

namespace ShipmentDeliveryAPI.Repositories.Interfaces
{
    public interface IShipmentRepository : IGenericRepository<Shipment>
    {
        Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber);
        Task<Shipment?> GetShipmentWithDeliveriesAsync(string shipmentNumber);
        Task<bool> ShipmentExistsAsync(string shipmentNumber);
    }
}

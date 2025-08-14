using ShipmentDeliveryAPI.Models;

namespace ShipmentDeliveryAPI.Repositories.Interfaces
{
    public interface IDeliveryRepository : IGenericRepository<Delivery>
    {
        Task<Delivery?> GetByDeliveryNumberAsync(string deliveryNumber);
        Task<Delivery?> GetDeliveryWithItemsAsync(string deliveryNumber);
        Task<Delivery?> GetDeliveryWithShipmentAsync(string deliveryNumber);
        Task<IEnumerable<Delivery>> GetDeliveriesByShipmentIdAsync(int shipmentId);
        Task<bool> DeliveryExistsAsync(string deliveryNumber);
    }
}

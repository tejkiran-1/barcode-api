using ShipmentDeliveryAPI.Models;

namespace ShipmentDeliveryAPI.Repositories.Interfaces
{
    public interface IContainerItemRepository : IGenericRepository<ContainerItem>
    {
        Task<IEnumerable<ContainerItem>> GetByDeliveryIdAsync(int deliveryId);
        Task<bool> ContainerItemExistsAsync(int deliveryId, string materialNumber, string serialNumber);
    }
}

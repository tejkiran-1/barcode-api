using ShipmentDeliveryAPI.DTOs;

namespace ShipmentDeliveryAPI.Services
{
    public interface IShipmentDeliveryService
    {
        Task<bool> CreateShipmentDeliveryAsync(CreateShipmentDeliveryRequestDto request);
        Task<ShipmentDeliveryResponseDto?> GetByShipmentNumberAsync(string shipmentNumber);
        Task<ShipmentDeliveryResponseDto?> GetByDeliveryNumberAsync(string deliveryNumber);
        Task<List<ShipmentDeliveryResponseDto>> GetAllShipmentsAsync();
        Task<bool> UpdateShipmentAsync(string currentShipmentNumber, UpdateShipmentRequestDto request);
        Task<bool> DeleteShipmentAsync(string shipmentNumber);
    }
}

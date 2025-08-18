using ShipmentDeliveryAPI.DTOs;

namespace ShipmentDeliveryAPI.Services
{
    public interface IShipmentDeliveryService
    {
        Task<bool> CreateShipmentDeliveryAsync(CreateShipmentDeliveryRequestDto request);
        Task<ShipmentDeliveryResponseDto?> GetByShipmentNumberAsync(string shipmentNumber);
        Task<ShipmentDeliveryResponseDto?> GetByDeliveryNumberAsync(string deliveryNumber);
        Task<PaginatedResponseDto<ShipmentDeliveryResponseDto>> GetAllShipmentsAsync(int page = 1, int pageSize = 20);
        Task<bool> UpdateShipmentAsync(string currentShipmentNumber, UpdateShipmentRequestDto request);
        Task<bool> DeleteShipmentAsync(string shipmentNumber);
    }
}

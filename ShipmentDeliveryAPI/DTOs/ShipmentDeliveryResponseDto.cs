using ShipmentDeliveryAPI.Models;

namespace ShipmentDeliveryAPI.DTOs
{
    public class ShipmentDeliveryResponseDto
    {
        public string ShipmentNumber { get; set; } = string.Empty;
        public DateTime ShipmentCreatedAt { get; set; }
        public List<DeliveryResponseDto> Deliveries { get; set; } = new List<DeliveryResponseDto>();
    }

    public class DeliveryResponseDto
    {
        public string DeliveryNumber { get; set; } = string.Empty;
        public DeliveryType DeliveryType { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ContainerItemResponseDto>? ContainerItems { get; set; }
        public List<BulkItemResponseDto>? BulkItems { get; set; }
    }

    public class ContainerItemResponseDto
    {
        public string MaterialNumber { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string ConnectionLabel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class BulkItemResponseDto
    {
        public string MaterialNumber { get; set; } = string.Empty;
        public string EvdSealNumber { get; set; } = string.Empty;
        public string ConnectionLabel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

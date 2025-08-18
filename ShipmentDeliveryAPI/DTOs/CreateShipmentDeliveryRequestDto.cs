using ShipmentDeliveryAPI.Models;

namespace ShipmentDeliveryAPI.DTOs
{
    public class CreateShipmentDeliveryRequestDto
    {
        public string ShipmentNumber { get; set; } = string.Empty;
        public string DeliveryNumber { get; set; } = string.Empty;
        public DeliveryType DeliveryType { get; set; }
        public List<ContainerItemDto>? ContainerItems { get; set; }
        public List<BulkItemDto>? BulkItems { get; set; }
    }

    public class ContainerItemDto
    {
        public string MaterialNumber { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string ConnectionLabel { get; set; } = string.Empty;
    }

    public class BulkItemDto
    {
        public string MaterialNumber { get; set; } = string.Empty;
        public string EvdSealNumber { get; set; } = string.Empty;
        public string ConnectionLabel { get; set; } = string.Empty;
    }
}

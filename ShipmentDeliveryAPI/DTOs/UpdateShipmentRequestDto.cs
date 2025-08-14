using System.ComponentModel.DataAnnotations;
using ShipmentDeliveryAPI.Models;

namespace ShipmentDeliveryAPI.DTOs
{
    public class UpdateShipmentRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string ShipmentNumber { get; set; } = string.Empty;

        public List<UpdateDeliveryDto>? Deliveries { get; set; }
    }

    public class UpdateDeliveryDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string DeliveryNumber { get; set; } = string.Empty;

        [Required]
        public DeliveryType DeliveryType { get; set; }

        public List<ContainerItemDto>? ContainerItems { get; set; }
        public List<BulkItemDto>? BulkItems { get; set; }
    }
}

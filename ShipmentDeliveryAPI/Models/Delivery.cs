using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipmentDeliveryAPI.Models
{
    public enum DeliveryType
    {
        Container,
        Bulk
    }

    public class Delivery
    {
        [Key]
        public int DeliveryId { get; set; }

        [Required]
        [StringLength(50)]
        public string DeliveryNumber { get; set; } = string.Empty;

        [Required]
        public DeliveryType DeliveryType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key to Shipment
        [Required]
        public int ShipmentId { get; set; }

        // Navigation properties
        [ForeignKey("ShipmentId")]
        public virtual Shipment Shipment { get; set; } = null!;

        // One delivery can have multiple container items (for container type)
        public virtual ICollection<ContainerItem> ContainerItems { get; set; } = new List<ContainerItem>();

        // One delivery can have multiple bulk items (for bulk type)
        public virtual ICollection<BulkItem> BulkItems { get; set; } = new List<BulkItem>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace ShipmentDeliveryAPI.Models
{
    public class Shipment
    {
        [Key]
        public int ShipmentId { get; set; }

        [Required]
        [StringLength(50)]
        public string ShipmentNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property - One shipment can have multiple deliveries
        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }
}

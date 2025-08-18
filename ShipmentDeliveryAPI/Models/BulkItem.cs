using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipmentDeliveryAPI.Models
{
    public class BulkItem
    {
        [Key]
        public int BulkItemId { get; set; }

        [Required]
        [StringLength(50)]
        public string MaterialNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EvdSealNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string ConnectionLabel { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key to Delivery
        [Required]
        public int DeliveryId { get; set; }

        // Navigation property
        [ForeignKey("DeliveryId")]
        public virtual Delivery Delivery { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class Payment : BaseEntity
    {
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public decimal? TotalAmount { get; set; }
        public decimal? Balance { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(250)]
        public string StatusMessage { get; set; } = string.Empty;

        // Foreign key to User
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }

        // linking to Event
        public int? EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}

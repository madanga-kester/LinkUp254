using LinkUp254.Features.Events;
using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Payment
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

        

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

    
        public int? EventId { get; set; }

        [ForeignKey("EventId")]
        public Events.Event? Event { get; set; }
    }
}
 
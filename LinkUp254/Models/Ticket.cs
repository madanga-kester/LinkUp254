using  System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class Ticket : BaseEntity
    {
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhoneNumber { get; set; }

        public string TicketType { get; set; } = "General Admission"; // e.g., General Admission, VIP, etc.
        public decimal? Price { get; set; } 
        public String SeatNumber { get; set; } = null!; // null for general admission tickets
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public int Quantity { get; set; } = 1;
           
        public string TicketCode { get; set; } = null!; // Unique code for the ticket
        public string? PaymentStatus { get; set; } // e.g., Paid, Pending, Free
        public string? TicketStatus { get; set; } // e.g., Active, Cancelled, Used

        public int TicketId { get; set; }

        
        public string BuyerId { get; set; }
        public string? PaymentId { get; set; } // null for free tickets
        
        public bool CheckedIn { get; set; } = false;


        public DateTime? CheckInTime { get; set; }   // when scanned at entry
        public string? CheckedInBy { get; set; }     // staff username or system


        public bool IsRefunded { get; set; } = false;
        public DateTime? RefundedAt { get; set; }
        public string? RefundReason { get; set; }



        public string? QRCodeImageUrl { get; set; }   // link to generated QR
        public string? VerificationStatus { get; set; } = "Unverified"; // "Verified", "Invalid"





        [ForeignKey ("Event")]
        public int EventId { get; set; } 
        public Event Event { get; set; } = null!;


    }
}

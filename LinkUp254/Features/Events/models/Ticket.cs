using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.models
{
    public class Ticket : BaseEntity
    {
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhoneNumber { get; set; }

        public string TicketType { get; set; } = "General Admission"; 
        public decimal? Price { get; set; } 
        public string SeatNumber { get; set; } = null!; 
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public int Quantity { get; set; } = 1;
           
        public string TicketCode { get; set; } = null!; 
        public string? PaymentStatus { get; set; } 
        public string? TicketStatus { get; set; } 

        public int TicketId { get; set; }

        
        public string BuyerId { get; set; }
        public string? PaymentId { get; set; } 

        public bool CheckedIn { get; set; } = false;


        public DateTime? CheckInTime { get; set; }   
        public string? CheckedInBy { get; set; }     


        public bool IsRefunded { get; set; } = false;
        public DateTime? RefundedAt { get; set; }
        public string? RefundReason { get; set; }



        public string? QRCodeImageUrl { get; set; }   
        public string? VerificationStatus { get; set; } = "Unverified"; // "Verified", "Invalid"





        [ForeignKey ("Event")]
        public int EventId { get; set; } 
        public Event Event { get; set; } = null!;


    }
}

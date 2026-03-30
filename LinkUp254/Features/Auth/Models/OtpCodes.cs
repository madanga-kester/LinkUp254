using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Auth.Models
{
    public class OtpCodes : BaseEntity
    {
      
        [Required]
        [StringLength(10)]
        public required string Code { get; set; }

        [Required]
        [StringLength(255)]
        public required string Identifier { get; set; }

       
        [Required]
        public DateTime ExpiresAt { get; set; }

       
        [Required]
        public bool IsUsed { get; set; } = false;

        
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

       
        [StringLength(50)]
        public string? OtpType { get; set; } = "General";

       
        public int AttemptCount { get; set; } = 0;
    }
}
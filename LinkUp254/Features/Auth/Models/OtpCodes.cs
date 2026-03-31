using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LinkUp254.Features.Shared;

namespace LinkUp254.Database
{
    public class OtpCodes
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Identifier { get; set; } = string.Empty;  // Email or Phone

        [Required]
        public string Purpose { get; set; } = "Standalone";  // "Signup", "Login", "PasswordReset", "Standalone"

        public string? OtpType { get; set; } = "Numeric";

        public int AttemptCount { get; set; } = 0;



        public User? User { get; set; }  // Reference to User entity

        

        [ForeignKey(nameof(User))]
        public int? UserId { get; set; }

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Auth.DTOs
{
    public class SendOtpDto
    {
       [Required]
        [StringLength(50)]
        public required string OTP { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }
    }
}
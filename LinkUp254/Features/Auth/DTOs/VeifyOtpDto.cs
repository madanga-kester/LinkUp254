using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Auth.DTOs
{
    
    public class VerifyOtpDto
    {
        [Required]
        [StringLength(50)]
        public required string OTP { get; set; }

        [Required]
        [StringLength(50)]
        public required string Code { get; set; }

        [Required]
        [RegularExpression("^(Signup|Login|PasswordReset|AdminLogin)$")]
        public required string Purpose { get; set; } // Why  OTP being verified?
    }
}
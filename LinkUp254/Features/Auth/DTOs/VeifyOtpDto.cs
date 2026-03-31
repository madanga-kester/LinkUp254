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
        [RegularExpression("^(Signup|Login|PasswordReset)$")]
        public required string Purpose { get; set; }  // reason Why  OTP being verified?
    }
}
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Auth.DTOs
{
    public class LoginDto
    {
        [Required][EmailAddress] public required string Email { get; set; }
        [Required] public required string Password { get; set; }
    }

    public class OtpLoginRequestDto
    {
        [Required][EmailAddress] public required string Email { get; set; }

        
        public string? Phone { get; set; }

        [Required]
        [RegularExpression("^(Email|Phone)$")]
        public required string OtpDeliveryMethod { get; set; }
    }

    public class VerifyOtpLoginDto
    {
        [Required][EmailAddress] public required string Email { get; set; }
        [Required] public required string Code { get; set; }
    }
}
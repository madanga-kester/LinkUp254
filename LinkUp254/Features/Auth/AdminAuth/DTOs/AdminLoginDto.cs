using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.AdminAuth.DTOs
{
    public class AdminLoginDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class AdminOtpLoginDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
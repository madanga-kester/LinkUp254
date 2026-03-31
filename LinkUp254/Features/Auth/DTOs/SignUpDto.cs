using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Auth.DTOs
{
    public class SignUpDto
    {
        [Required]
        [StringLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [Phone]
        public required string Phone { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must contain uppercase, lowercase, number and special character")]
        public required string Password { get; set; }



        [Required]
        [RegularExpression("^(Email|Phone)$", ErrorMessage = "Must be 'Email' or 'Phone'")]
        public required string OtpDeliveryMethod { get; set; } 
    }
}
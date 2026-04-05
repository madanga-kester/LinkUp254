


using System;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Auth.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }  

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }

        public string? Bio { get; set; }
        public string? Website { get; set; }

        public string? ProfilePicture { get; set; }  

        public DateTime? DateOfBirth { get; set; }

        public int? Age { get; set; } 
    }
}
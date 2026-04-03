//using System.ComponentModel.DataAnnotations;

//namespace LinkUp254.Features.Auth.DTOs
//{
//    public class UpdateProfileDto
//    {
//        [Required]
//        [EmailAddress]
//        public required string Email { get; set; }  //  for lookup

//        [StringLength(100)]
//        public string? FirstName { get; set; }

//        [StringLength(100)]
//        public string? LastName { get; set; }

//        public DateTime? DateOfBirth { get; set; }

//        [Phone]
//        public string? PhoneNumber { get; set; }

//        [StringLength(500)]
//        public string? Address { get; set; }

//        [StringLength(100)]
//        public string? City { get; set; }

//        [StringLength(100)]
//        public string? Country { get; set; }

//        [StringLength(100)]
//        public string? State { get; set; }

//        [StringLength(20)]
//        public string? ZipCode { get; set; }

//        [StringLength(1000)]
//        public string? Bio { get; set; }

//        [Url]
//        public string? Website { get; set; }
//    }
//}


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
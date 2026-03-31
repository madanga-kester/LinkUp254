using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Auth.DTOs
{
    public class RefreshTokenDto
    {
        [Required] public required string Token { get; set; }
        [Required] public required string RefreshToken { get; set; }
    }
}
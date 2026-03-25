namespace LinkUp254.Features.Auth.DTOs
{
    public class VerifyPassDto
    {
        public required string OTP { get; set; }
        public required string Password { get; set; }
    }
}

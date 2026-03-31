namespace LinkUp254.Features.Auth.DTOs
{
    public class AuthResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }          // JWT token
        public string? RefreshToken { get; set; }   
        public UserDto? User { get; set; }
        public bool RequiresOtpVerification { get; set; }  // True if signup needs OTP
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }
}
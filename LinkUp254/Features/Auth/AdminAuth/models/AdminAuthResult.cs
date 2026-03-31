using LinkUp254.Features.AdminAuth.DTOs;

namespace LinkUp254.Features.AdminAuth.Models
{
    
    public class AdminAuthResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }          
        public string? RefreshToken { get; set; }  
        public AdminUserDto? User { get; set; }    
        public bool RequiresOtpVerification { get; set; }

        
        public static AdminAuthResult Success(string message) =>
            new AdminAuthResult { IsSuccess = true, Message = message };

        public static AdminAuthResult Success(string token, AdminUserDto user) =>
            new AdminAuthResult
            {
                IsSuccess = true,
                Token = token,
                User = user,
                Message = "Admin login successful"
            };

        public static AdminAuthResult Failure(string message) =>
            new AdminAuthResult { IsSuccess = false, Message = message };
    }
}
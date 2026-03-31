using LinkUp254.Features.AdminAuth.DTOs;
using LinkUp254.Features.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LinkUp254.Features.AdminAuth
{
    [ApiController]
    [Route("api/admin/auth")] 
    public class AdminAuthController : ControllerBase
    {
        private readonly AdminAuthServices _adminAuthServices;

        public AdminAuthController(AdminAuthServices adminAuthServices)
        {
            _adminAuthServices = adminAuthServices;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });

            var result = await _adminAuthServices.LoginAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("otp-login-request")]
        public async Task<IActionResult> RequestOtpLogin([FromBody] AdminOtpLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });

            var result = await _adminAuthServices.RequestOtpLoginAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("otp-login-verify")]
        public async Task<IActionResult> VerifyOtpLogin([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });

            var result = await _adminAuthServices.VerifyOtpLoginAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { IsSuccess = true, Message = "Admin logged out." });
        }
    }
}
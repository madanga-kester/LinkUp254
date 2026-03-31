using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LinkUp254.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace LinkUp254.Features.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthServices _authServices;

        public AuthController(AuthServices authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.RegisterAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.SignUpAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("activate-account")]
        public async Task<IActionResult> ActivateAccount([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.ActivateAccountAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.LoginAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("otp-login-request")]
        public async Task<IActionResult> RequestOtpLogin([FromBody] OtpLoginRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.RequestOtpLoginAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("otp-login-verify")]
        public async Task<IActionResult> VerifyOtpLogin([FromBody] VerifyOtpLoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.VerifyOtpLoginAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.SendOtpAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.VerifyOtpAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPassDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.VerifyPasswordAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.UpdateProfileAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = ModelState });
            var result = await _authServices.RefreshTokenAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LinkUp254.Features.Auth.DTOs;
using LinkUp254.Features.Auth; 
using System.Net.Mime;

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
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authServices.RegisterAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = result.Message });
            return Ok(new { Message = result.Message, Token = result.Token, User = new { result.User.Email, result.User.FirstName } });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authServices.SignUpAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = result.Message });
            return Ok(new { Message = result.Message, Token = result.Token });
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authServices.SendOtpAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = result.Message });
            return Ok(new { Message = result.Message });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authServices.VerifyOtpAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = result.Message });
            return Ok(new { Message = result.Message });
        }

        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPassDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authServices.VerifyPasswordAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = result.Message });
            return Ok(new { Message = result.Message });
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authServices.UpdateProfileAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = result.Message });
            return Ok(new { Message = result.Message, User = new { result.User.Email, result.User.FirstName } });
        }
    }
}
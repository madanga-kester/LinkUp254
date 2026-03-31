using LinkUp254.Database;
using LinkUp254.Features.AdminAuth.DTOs;
using LinkUp254.Features.AdminAuth.Models;
using LinkUp254.Features.Auth.DTOs;
using LinkUp254.Features.Shared;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LinkUp254.Features.AdminAuth
{
    public class AdminAuthServices
    {
        private readonly LinkUpContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AdminAuthServices> _logger;
        private readonly IConfiguration _config;

        public AdminAuthServices(
            LinkUpContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<AdminAuthServices> logger,
            IConfiguration config)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _config = config;
        }

        public async Task<AdminAuthResult> LoginAsync(AdminLoginDto dto)
        {
            try
            {
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Role == "Admin");
                if (admin == null) return AdminAuthResult.Failure("Admin account not found.");
                if (!admin.IsActive) return AdminAuthResult.Failure("Admin account not activated.");

                var result = _passwordHasher.VerifyHashedPassword(admin, admin.Password, dto.Password);
                if (result != PasswordVerificationResult.Success) return AdminAuthResult.Failure("Invalid admin password.");

                var token = GenerateAdminJwtToken(admin);
                var refreshToken = GenerateRefreshToken();
                var adminUserDto = new AdminUserDto { Id = admin.Id, Email = admin.Email, FirstName = admin.FirstName, LastName = admin.LastName, PhoneNumber = admin.PhoneNumber, Role = "Admin" };

                _logger.LogInformation("Admin logged in: {Email}", admin.Email);
                return AdminAuthResult.Success(token, adminUserDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin login failed for {Email}", dto.Email);
                return AdminAuthResult.Failure($"Admin login failed: {ex.Message}");
            }
        }

        public async Task<AdminAuthResult> RequestOtpLoginAsync(AdminOtpLoginDto dto)
        {
            try
            {
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Role == "Admin");
                if (admin == null) return AdminAuthResult.Failure("Admin account not found.");
                if (!admin.IsActive) return AdminAuthResult.Failure("Admin account not activated.");

                var otpCode = new Random().Next(100000, 999999).ToString();
                var otpRecord = new OtpCodes { Code = otpCode, Identifier = admin.Email, Purpose = "AdminLogin", ExpiresAt = DateTime.UtcNow.AddMinutes(5), IsUsed = false, CreatedAt = DateTime.UtcNow };

                await _context.OtpCodes.AddAsync(otpRecord);
                await _context.SaveChangesAsync();
                await SendAdminOtpEmailAsync(admin.Email, otpCode);

                _logger.LogInformation("Admin OTP login requested: {Email}", admin.Email);
                return AdminAuthResult.Success("Admin OTP sent to email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin OTP request failed for {Email}", dto.Email);
                return AdminAuthResult.Failure($"Failed to send admin OTP: {ex.Message}");
            }
        }

        public async Task<AdminAuthResult> VerifyOtpLoginAsync(VerifyOtpDto dto)
        {
            try
            {
                var otpRecord = await _context.OtpCodes.FirstOrDefaultAsync(o =>
                    o.Code == dto.Code &&
                    o.Identifier == dto.OTP &&
                    o.Purpose == "AdminLogin" &&
                    !o.IsUsed &&
                    o.ExpiresAt > DateTime.UtcNow);

                if (otpRecord == null) return AdminAuthResult.Failure("Invalid or expired admin OTP.");

                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.OTP && u.Role == "Admin");
                if (admin == null) return AdminAuthResult.Failure("Admin account not found.");

                otpRecord.IsUsed = true;
                _context.OtpCodes.Update(otpRecord);
                await _context.SaveChangesAsync();

                var token = GenerateAdminJwtToken(admin);
                var refreshToken = GenerateRefreshToken();
                var adminUserDto = new AdminUserDto { Id = admin.Id, Email = admin.Email, FirstName = admin.FirstName, LastName = admin.LastName, PhoneNumber = admin.PhoneNumber, Role = "Admin" };

                _logger.LogInformation("Admin OTP login successful: {Email}", admin.Email);
                return AdminAuthResult.Success(token, adminUserDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin OTP verification failed for {OTP}. Details: {Message}", dto.OTP, ex.Message);
                return AdminAuthResult.Failure($"Admin login failed: {ex.Message}");
            }
        }

        private string GenerateAdminJwtToken(User admin)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, admin.Email),
                new Claim("FirstName", admin.FirstName),
                new Claim("LastName", admin.LastName),
                new Claim("Role", "Admin"),
                new Claim("IsAdmin", "true"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expirationMinutes = int.Parse(jwtSettings["AdminExpirationMinutes"] ?? "120");
            var token = new JwtSecurityToken(issuer: jwtSettings["Issuer"], audience: jwtSettings["Audience"], claims: claims, expires: DateTime.UtcNow.AddMinutes(expirationMinutes), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task SendAdminOtpEmailAsync(string toEmail, string otp)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["MailSettings:From"]!));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "LinkUp254 Admin OTP Code";
            email.Body = new TextPart("html") { Text = $@"<html><body style='font-family:Arial,sans-serif'><h2>🔐 Admin Verification</h2><p>Your admin one-time code is:</p><p style='font-size:28px;letter-spacing:8px;font-weight:bold'>{otp}</p><p style='color:#666'>This code expires in 5 minutes.</p></body></html>" };

            using var smtp = new SmtpClient();
            if (_config["ASPNETCORE_ENVIRONMENT"] == "Development") smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await smtp.ConnectAsync(_config["MailSettings:Host"]!, int.Parse(_config["MailSettings:Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["MailSettings:Username"]!, _config["MailSettings:Password"]!);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
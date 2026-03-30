using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LinkUp254.Features.Auth.DTOs;
using LinkUp254.Database;
using LinkUp254.Features.Shared;
using LinkUp254.Features.Auth.Models;
using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.EntityFrameworkCore;

namespace LinkUp254.Features.Auth
{
    public class AuthServices
    {
        private readonly LinkUpContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthServices> _logger;
        private readonly IConfiguration _config;

        public AuthServices(
            LinkUpContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<AuthServices> logger,
            IConfiguration config)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _config = config;
        }

        public async Task<AuthResult> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser != null)
                    return AuthResult.Failure("User with this email already exists.");

                var hashedPassword = _passwordHasher.HashPassword(null!, dto.Password);

                var user = new User
                {
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber, 
                    Password = hashedPassword,     
                    Role = "User",                 
                    Age = dto.Age,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var token = $"temp_token_{user.Id}_{DateTime.UtcNow.Ticks}";

                _logger.LogInformation("User registered successfully: {Email}", dto.Email);
                return AuthResult.Success(token, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
                return AuthResult.Failure("Registration failed.");
            }
        }

        public async Task<AuthResult> SignUpAsync(SignUpDto dto)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser != null)
                    return AuthResult.Failure("User with this email already exists.");

                
                var hashedPassword = _passwordHasher.HashPassword(null!, dto.Password);

                var user = new User
                {
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.Phone,    
                    Password = hashedPassword,  
                    Role = "User",              
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var token = $"temp_token_{user.Id}_{DateTime.UtcNow.Ticks}";

                _logger.LogInformation("User signed up successfully: {Email}", dto.Email);
                return AuthResult.Success(token, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignUp failed for {Email}", dto.Email);
                return AuthResult.Failure("SignUp failed.");
            }
        }

        public async Task<AuthResult> SendOtpAsync(SendOtpDto dto)
        {
            try
            {
                var generatedOtp = new Random().Next(100000, 999999).ToString();
                var identifier = dto.OTP;

                var otpRecord = new OtpCodes
                {
                    Code = generatedOtp,
                    Identifier = identifier,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.OtpCodes.AddAsync(otpRecord);
                await _context.SaveChangesAsync();

                await SendEmailAsync(identifier, generatedOtp);

                _logger.LogInformation("OTP sent successfully to {Identifier}", identifier);
                return AuthResult.Success("OTP sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendOtp failed.");
                return AuthResult.Failure("Failed to send OTP.");
            }
        }

        public async Task<AuthResult> VerifyOtpAsync(VerifyOtpDto dto)
        {
            try
            {
                var otpRecord = await _context.OtpCodes
                    .FirstOrDefaultAsync(o => o.Code == dto.Code && o.Identifier == dto.OTP && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

                if (otpRecord == null)
                    return AuthResult.Failure("Invalid or expired OTP.");

                otpRecord.IsUsed = true;
                _context.OtpCodes.Update(otpRecord);
                await _context.SaveChangesAsync();

                return AuthResult.Success("OTP verified.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyOtp failed.");
                return AuthResult.Failure("Failed to verify OTP.");
            }
        }

        public async Task<AuthResult> VerifyPasswordAsync(VerifyPassDto dto)
        {
            try
            {
                var otpRecord = await _context.OtpCodes
                    .FirstOrDefaultAsync(o => o.Code == dto.OTP && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

                if (otpRecord == null)
                    return AuthResult.Failure("Invalid OTP for password verification.");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == otpRecord.Identifier);
                if (user == null)
                    return AuthResult.Failure("User not found.");

                var hashedPassword = _passwordHasher.HashPassword(user, dto.Password);
                user.Password = hashedPassword;

                otpRecord.IsUsed = true;
                _context.OtpCodes.Update(otpRecord);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return AuthResult.Success("Password updated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyPassword failed.");
                return AuthResult.Failure("Failed to verify password.");
            }
        }

        public async Task<AuthResult> UpdateProfileAsync(UpdateProfileDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return AuthResult.Failure("User not found.");

                if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
                if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
                if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth.Value;
                if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
                if (!string.IsNullOrEmpty(dto.Address)) user.Address = dto.Address;
                if (!string.IsNullOrEmpty(dto.City)) user.City = dto.City;
                if (!string.IsNullOrEmpty(dto.Country)) user.Country = dto.Country;
                if (!string.IsNullOrEmpty(dto.State)) user.State = dto.State;
                if (!string.IsNullOrEmpty(dto.ZipCode)) user.ZipCode = dto.ZipCode;
                if (!string.IsNullOrEmpty(dto.Bio)) user.Bio = dto.Bio;
                if (!string.IsNullOrEmpty(dto.Website)) user.Website = dto.Website;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return AuthResult.Success("Profile updated.", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProfile failed for {Email}", dto.Email);
                return AuthResult.Failure("Failed to update profile.");
            }
        }

        private async Task SendEmailAsync(string toEmail, string otp)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["MailSettings:From"]!));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Your LinkUp254 OTP Code";
            email.Body = new TextPart("plain") { Text = $"Your OTP is: {otp}" };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["MailSettings:Host"]!,
                int.Parse(_config["MailSettings:Port"]!),
                MailKit.Security.SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_config["MailSettings:Username"]!, _config["MailSettings:Password"]!);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public User? User { get; set; }

        public static AuthResult Success(string message) =>
            new AuthResult { IsSuccess = true, Message = message };

        public static AuthResult Success(string token, User user) =>
            new AuthResult { IsSuccess = true, Token = token, User = user, Message = "Operation successful" };

        public static AuthResult Failure(string message) =>
            new AuthResult { IsSuccess = false, Message = message };
    }
}
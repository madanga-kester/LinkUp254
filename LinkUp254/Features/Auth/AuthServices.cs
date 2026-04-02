using LinkUp254.Database;
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

        //  REGISTER - for backward compatibility
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
                    IsActive = true,
                    Age = dto.Age,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                _logger.LogInformation("User registered successfully: {Email}", dto.Email);
                return new AuthResult
                {
                    IsSuccess = true,
                    Message = "Registration successful.",
                    Token = token,
                    RefreshToken = refreshToken,
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
                return AuthResult.Failure("Registration failed.");
            }
        }

        //  SIGNUP 
        public async Task<AuthResult> SignUpAsync(SignUpDto dto)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser != null)
                {
                    if (existingUser.IsActive)
                        return AuthResult.Failure("User with this email already exists.");
                    return AuthResult.Failure("Account exists but not verified. Please verify OTP.");
                }

                var hashedPassword = _passwordHasher.HashPassword(null!, dto.Password);
                var user = new User
                {
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.Phone,
                    Password = hashedPassword,
                    Role = "User",
                    IsActive = false,  //  Inactive until OTP verified
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Determine identifier based on delivery method
                var identifier = dto.OtpDeliveryMethod?.ToLower() == "email" ? dto.Email : dto.Phone;
                if (string.IsNullOrEmpty(identifier))
                    return AuthResult.Failure($"Please provide a valid {dto.OtpDeliveryMethod?.ToLower()} for OTP.");

                var otpCode = await GenerateAndSaveOtpAsync(identifier, "Signup");

                // Send OTP with error handling - don't fail signup if email/SMS fails
                try
                {
                    await SendOtpToIdentifierAsync(identifier, otpCode, dto.OtpDeliveryMethod ?? "Email");
                    _logger.LogInformation("OTP sent successfully to {Identifier} via {Method}", identifier, dto.OtpDeliveryMethod);
                }
                catch (Exception otpEx)
                {
                    _logger.LogError(otpEx, "FAILED to send OTP to {Identifier} via {Method} - user can request resend", identifier, dto.OtpDeliveryMethod);
                  
                }

                _logger.LogInformation("User registered (inactive): {Email}. OTP sent to {Method}", dto.Email, dto.OtpDeliveryMethod);
                return AuthResult.Success("Account created. Please verify OTP to activate.", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
                return AuthResult.Failure("Registration failed.");
            }
        }

        //  ACTIVATE ACCOUNT VIA OTP 
        public async Task<AuthResult> ActivateAccountAsync(VerifyOtpDto dto)
        {
            try
            {
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.OTP || u.PhoneNumber == dto.OTP);
                if (user == null)
                    return AuthResult.Failure("User not found.");

                if (user.IsActive)
                    return AuthResult.Failure("Account already activated.");

                

                var otpRecord = await _context.OtpCodes
                    .FirstOrDefaultAsync(o => o.Code == dto.Code
                        && o.Identifier == dto.OTP
                        && o.Purpose == "Signup"
                        && !o.IsUsed
                        && o.ExpiresAt > DateTime.UtcNow);

                if (otpRecord == null)
                    return AuthResult.Failure("Invalid or expired OTP.");

              

                user.IsActive = true;
                otpRecord.IsUsed = true;

                _context.Users.Update(user);
                _context.OtpCodes.Update(otpRecord);
                await _context.SaveChangesAsync();

                // Generate JWT tokens
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                _logger.LogInformation("Account activated: {Email}", user.Email);
                return new AuthResult
                {
                    IsSuccess = true,
                    Message = "Account activated successfully.",
                    Token = token,
                    RefreshToken = refreshToken,
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account activation failed for {OTP}", dto.OTP);
                return AuthResult.Failure("Activation failed.");
            }
        }

        //   LOGIN wiith  PASS
        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return AuthResult.Failure("User not found.");

                if (!user.IsActive)
                    return AuthResult.Failure("Account not activated. Please verify OTP.");

                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, dto.Password);
                if (result != PasswordVerificationResult.Success)
                    return AuthResult.Failure("Invalid password.");

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                _logger.LogInformation("User logged in: {Email}", dto.Email);
                return new AuthResult
                {
                    IsSuccess = true,
                    Message = "Login successful.",
                    Token = token,
                    RefreshToken = refreshToken,
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", dto.Email);
                return AuthResult.Failure("Login failed.");
            }
        }

        //  OTP LOGIN - REQUEST
        //public async Task<AuthResult> RequestOtpLoginAsync(OtpLoginRequestDto dto)
        //{
        //    try
        //    {
        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        //        if (user == null)
        //            return AuthResult.Failure("User not found.");

        //        if (!user.IsActive)
        //            return AuthResult.Failure("Account not activated.");



        //        var identifier = dto.OtpDeliveryMethod?.ToLower() == "email" ? dto.Email : user.PhoneNumber;
        //        if (string.IsNullOrEmpty(identifier))
        //            return AuthResult.Failure($"{dto.OtpDeliveryMethod} not available for this account.");

        //        var otpCode = await GenerateAndSaveOtpAsync(identifier, "Login");

        //        // Send OTP with error handling
        //        try
        //        {
        //            await SendOtpToIdentifierAsync(identifier, otpCode, dto.OtpDeliveryMethod ?? "Email");
        //            _logger.LogInformation("OTP sent successfully to {Identifier} via {Method}", identifier, dto.OtpDeliveryMethod);
        //        }
        //        catch (Exception otpEx)
        //        {
        //            _logger.LogError(otpEx, "FAILED to send OTP to {Identifier} via {Method}", identifier, dto.OtpDeliveryMethod);
        //            return AuthResult.Failure("Failed to send OTP. Please try again.");
        //        }

        //        _logger.LogInformation("OTP login requested: {Email} via {Method}", dto.Email, dto.OtpDeliveryMethod);
        //        return AuthResult.Success($"OTP sent to {dto.OtpDeliveryMethod?.ToLower()}.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "OTP login request failed for {Email}", dto.Email);
        //        return AuthResult.Failure("Failed to send OTP.");
        //    }
        //}




        //  OTP LOGIN - REQUEST
        public async Task<AuthResult> RequestOtpLoginAsync(OtpLoginRequestDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return AuthResult.Failure("User not found.");

                if (!user.IsActive)
                    return AuthResult.Failure("Account not activated.");

                
                if (dto.OtpDeliveryMethod?.ToLower() == "phone")
                {
                    // Check if user has a phone number in database
                    if (string.IsNullOrEmpty(user.PhoneNumber))
                        return AuthResult.Failure("No phone number linked to this account.");

                    
                    if (!string.IsNullOrEmpty(dto.Phone))
                    {
                        // Normalize both numbers for comparison 
                        var inputPhone = dto.Phone.Replace(" ", "").Replace("-", "").Replace("+", "");
                        var dbPhone = user.PhoneNumber.Replace(" ", "").Replace("-", "").Replace("+", "");

                        if (inputPhone != dbPhone)
                        {
                            _logger.LogWarning("Phone mismatch: Input={InputPhone}, Database={DbPhone} for Email={Email}",
                                dto.Phone, user.PhoneNumber, dto.Email);
                            return AuthResult.Failure("This phone number is not linked to the provided email address.");
                        }
                    }
                }

                //  Using the phone from DATABASE (not user input) to ensure security
                var identifier = dto.OtpDeliveryMethod?.ToLower() == "email" ? dto.Email : user.PhoneNumber;
                if (string.IsNullOrEmpty(identifier))
                    return AuthResult.Failure($"{dto.OtpDeliveryMethod} not available for this account.");

                var otpCode = await GenerateAndSaveOtpAsync(identifier, "Login");

                // Sending OTP with error handling
                try
                {
                    await SendOtpToIdentifierAsync(identifier, otpCode, dto.OtpDeliveryMethod ?? "Email");
                    _logger.LogInformation("OTP sent successfully to {Identifier} via {Method}", identifier, dto.OtpDeliveryMethod);
                }
                catch (Exception otpEx)
                {
                    _logger.LogError(otpEx, "FAILED to send OTP to {Identifier} via {Method}", identifier, dto.OtpDeliveryMethod);
                    return AuthResult.Failure("Failed to send OTP. Please try again.");
                }

                _logger.LogInformation("OTP login requested: {Email} via {Method}", dto.Email, dto.OtpDeliveryMethod);
                return AuthResult.Success($"OTP sent to {dto.OtpDeliveryMethod?.ToLower()}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OTP login request failed for {Email}", dto.Email);
                return AuthResult.Failure("Failed to send OTP.");
            }
        }


        //  OTP LOGIN - VERIFY 
        public async Task<AuthResult> VerifyOtpLoginAsync(VerifyOtpLoginDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return AuthResult.Failure("User not found.");

                if (!user.IsActive)
                    return AuthResult.Failure("Account not activated.");

                
                var otpRecord = await _context.OtpCodes
                    .FirstOrDefaultAsync(o => o.Code == dto.Code
                        && (o.Identifier == dto.Email || o.Identifier == user.PhoneNumber)  
                        && o.Purpose == "Login"
                        && !o.IsUsed
                        && o.ExpiresAt > DateTime.UtcNow);

                if (otpRecord == null)
                    return AuthResult.Failure("Invalid or expired OTP.");

                otpRecord.IsUsed = true;
                _context.OtpCodes.Update(otpRecord);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                _logger.LogInformation("OTP login successful: {Email}", dto.Email);
                return new AuthResult
                {
                    IsSuccess = true,
                    Message = "Login successful.",
                    Token = token,
                    RefreshToken = refreshToken,
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OTP login verification failed for {Email}", dto.Email);
                return AuthResult.Failure("Login failed.");
            }
        }



        //  SEND OTP (Standalone) 
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
                    Purpose = "Standalone",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.OtpCodes.AddAsync(otpRecord);
                await _context.SaveChangesAsync();

                await SendOtpToIdentifierAsync(identifier, generatedOtp, "Email");

                _logger.LogInformation("OTP sent successfully to {Identifier}", identifier);
                return AuthResult.Success("OTP sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendOtp failed.");
                return AuthResult.Failure("Failed to send OTP.");
            }
        }

        //  VERIFY OTP (Standalone) 
        public async Task<AuthResult> VerifyOtpAsync(VerifyOtpDto dto)
        {
            try
            {
                var otpRecord = await _context.OtpCodes
                    .FirstOrDefaultAsync(o => o.Code == dto.Code
                        && o.Identifier == dto.OTP
                        && !o.IsUsed
                        && o.ExpiresAt > DateTime.UtcNow);

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


        //  VERIFY PASSWORD (Reset) 
        public async Task<AuthResult> VerifyPasswordAsync(VerifyPassDto dto)
        {
            try
            {
                
                var otpRecord = await _context.OtpCodes
                    .FirstOrDefaultAsync(o => o.Code == dto.OTP && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

                if (otpRecord == null)
                    return AuthResult.Failure("Invalid or expired reset code.");

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





        // USER UPDATE PROFILE 
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

        //  REFRESH TOKEN 
        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenDto dto)
        {
            try
            {
                
                return AuthResult.Failure("Refresh token logic not fully implemented yet, will do so later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken failed");
                return AuthResult.Failure("Token refresh failed.");
            }
        }

        //  HELPERS 
        private async Task<string> GenerateAndSaveOtpAsync(string identifier, string purpose)
        {
            var otpCode = new Random().Next(100000, 999999).ToString();

            var otpRecord = new OtpCodes
            {
                Code = otpCode,
                Identifier = identifier,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.OtpCodes.AddAsync(otpRecord);
            await _context.SaveChangesAsync();

            return otpCode;
        }

        private async Task SendOtpToIdentifierAsync(string identifier, string otpCode, string deliveryMethod)
        {
            if (deliveryMethod.ToLower() == "email")
            {
                await SendEmailAsync(identifier, otpCode);
            }
            else if (deliveryMethod.ToLower() == "phone")
            {
                await SendSmsAsync(identifier, otpCode);
            }
        }

        private async Task SendEmailAsync(string toEmail, string otp)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["MailSettings:From"]!));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Your LinkUp254 OTP Code";
            email.Body = new TextPart("html")
            {
                Text = $@"<html><body style='font-family:Arial,sans-serif'><h2>🔐 LinkUp254 Verification</h2><p>Your one-time code is:</p><p style='font-size:28px;letter-spacing:8px;font-weight:bold'>{otp}</p><p style='color:#666'>This code expires in 5 minutes.</p><p style='color:#999;font-size:12px'>If you didn't request this, please ignore this email.</p></body></html>"
            };

            using var smtp = new SmtpClient();

            // DEV MODE: Bypass SSL certificate validation for Mailtrap/Self-signed certs
            if (_config["ASPNETCORE_ENVIRONMENT"] == "Development")
            {
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            }

            await smtp.ConnectAsync(
                _config["MailSettings:Host"]!,
                int.Parse(_config["MailSettings:Port"]!),
                MailKit.Security.SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_config["MailSettings:Username"]!, _config["MailSettings:Password"]!);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        private async Task SendSmsAsync(string phoneNumber, string otpCode)
        {
            // MOCK FOR DEVELOPMENT - logs OTP to console instead of sending real SMS
            _logger.LogInformation("📱 SMS OTP for {Phone}: {Code} (MOCK - no real SMS sent in dev)", phoneNumber, otpCode);

            // To enable real SMS, uncomment ONE of the options below:

            //  OPTION 1: Africa's Talking (Kenya) 
            /*
            var apiKey = _config["AfricaTalking:ApiKey"];
            var username = _config["AfricaTalking:Username"];
            var url = "https://api.africastalking.com/version1/messaging";
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("ApiKey", apiKey);
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("to", phoneNumber),
                new KeyValuePair<string, string>("message", $"Your LinkUp254 OTP: {otpCode}. Valid for 5 minutes.")
            });
            
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            */

            //  OPTION 2: Twilio (Global) 
            /*
            // Install: dotnet add package Twilio
            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];
            var fromNumber = _config["Twilio:FromNumber"];
            
            TwilioClient.Init(accountSid, authToken);
            await MessageResource.CreateAsync(
                body: $"Your LinkUp254 OTP: {otpCode}. Valid for 5 minutes.",
                from: new Twilio.Types.PhoneNumber(fromNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );
            */
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("Role", user.Role ?? "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    // AuthResult class 
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
        public bool RequiresOtpVerification { get; set; }

        public static AuthResult Success(string message) =>
            new AuthResult { IsSuccess = true, Message = message };

        public static AuthResult Success(string token, User user) =>
            new AuthResult { IsSuccess = true, Token = token, User = user, Message = "Operation successful" };

        public static AuthResult Failure(string message) =>
            new AuthResult { IsSuccess = false, Message = message };
    }
}
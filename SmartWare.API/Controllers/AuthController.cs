using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartWare.API.Core.Common;
using SmartWare.API.Core.Entities;
using SmartWare.API.Data;
using SmartWare.API.Features.Auth.DTOs;
using SmartWare.API.Features.Auth.Services;

namespace SmartWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            ApplicationDbContext context,
            ITokenService tokenService,
            IMapper mapper,
            ILogger<AuthController> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }

        // ============================================
        // POST: api/auth/register
        // Yeni kullanıcı kaydı
        // ============================================
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            try
            {
                // Username unique mi?
                if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                // Email unique mi?
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                // Şifreyi hash'le (basit versiyon - production'da BCrypt kullan!)
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

                // User oluştur
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Role = "Reader", // Default role
                    IsActive = true,
                    EmailConfirmed = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Token oluştur
                var token = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Refresh token'ı veritabanına kaydet
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    User = userDto
                };

                _logger.LogInformation("User registered: {Username}", user.Username);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // POST: api/auth/login
        // Kullanıcı girişi
        // ============================================
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                // Username veya Email ile kullanıcı bul
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Username == loginDto.UsernameOrEmail ||
                        u.Email == loginDto.UsernameOrEmail);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Şifre doğrula
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Aktif mi?
                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Account is deactivated" });
                }

                // Token oluştur
                var token = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Refresh token'ı güncelle
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    User = userDto
                };

                _logger.LogInformation("User logged in: {Username}", user.Username);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // POST: api/auth/refresh
        // Token yenileme
        // ============================================
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                // Refresh token ile user bul
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid refresh token" });
                }

                // Refresh token süresi dolmuş mu?
                if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return Unauthorized(new { message = "Refresh token expired" });
                }

                // Yeni token oluştur
                var token = _tokenService.GenerateAccessToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Refresh token'ı güncelle
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = newRefreshToken,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    User = userDto
                };

                _logger.LogInformation("Token refreshed for user: {Username}", user.Username);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

// ============================================
// AÇIKLAMALAR
// ============================================

// 1. BCrypt.Net.BCrypt:
//    - Şifre hash'leme
//    - HashPassword() → Şifreyi hash'le
//    - Verify() → Şifreyi doğrula

// 2. Register:
//    - Username/Email unique kontrolü
//    - Şifre hash'leme
//    - User oluşturma
//    - Token + RefreshToken üretme

// 3. Login:
//    - Username VEYA Email ile giriş
//    - Şifre doğrulama
//    - IsActive kontrolü
//    - Token üretme

// 4. RefreshToken:
//    - Refresh token ile yeni token alma
//    - Süre kontrolü
//    - Security (refresh token rotation)

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartWare.API.Core.Common;
using SmartWare.API.Core.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartWare.API.Features.Auth.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateAccessToken(User user)
        {

            // Claims (kullanıcı bilgileri)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("firstName", user.FirstName ?? ""),
                new Claim("lastName", user.LastName ?? "")
            };
            // Secret Key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token oluştur
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Refresh Token oluşturur (random string)
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        /// <summary>
        /// Expired token'dan claims çıkarır (refresh token için)
        /// </summary>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)

        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateLifetime = false // Expired token'ı kabul et
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
    }
}

// ============================================
// AÇIKLAMALAR
// ============================================

// 1. Claims:
//    - Token içinde saklanan kullanıcı bilgileri
//    - ClaimTypes.NameIdentifier → User ID
//    - ClaimTypes.Role → "Admin", "Author", "Reader"

// 2. SymmetricSecurityKey:
//    - SecretKey ile token imzalama
//    - HmacSha256 algorithm

// 3. JwtSecurityToken:
//    - Issuer: Kim oluşturdu?
//    - Audience: Kimin için?
//    - Expires: Ne zaman geçersiz olacak?

// 4. GenerateRefreshToken:
//    - 32 byte random string
//    - Veritabanında saklanır
//    - Token yenilemek için kullanılır

// 5. GetPrincipalFromExpiredToken:
//    - Süresi dolmuş token'ı okur
//    - Claims'leri çıkarır
//    - Refresh token işlemi için
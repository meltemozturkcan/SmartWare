using System.ComponentModel.DataAnnotations;

namespace SmartWare.API.Features.Auth.DTOs
{
    /// <summary>
    /// Token yenileme için DTO
    /// </summary>
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
// ============================================
// AÇIKLAMALAR
// ============================================

// 1. RegisterDto:
//    - Password + ConfirmPassword validation
//    - [Compare] attribute ile eşleşme kontrolü
//    - MinimumLength ile güvenlik

// 2. LoginDto:
//    - Username VEYA Email ile giriş
//    - Flexible login

// 3. AuthResponseDto:
//    - Token (JWT)
//    - RefreshToken (token yenileme için)
//    - User bilgileri (PasswordHash YOK!)

// 4. UserDto:
//    - Güvenli user bilgileri
//    - PasswordHash, RefreshToken gibi hassas bilgiler YOK

// 5. RefreshTokenDto:
//    - Refresh token ile yeni token alma
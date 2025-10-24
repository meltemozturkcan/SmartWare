using System.ComponentModel.DataAnnotations;

namespace SmartWare.API.Features.Auth.DTOs
{
    /// <summary>
    /// Kullanıcı giriş için DTO
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Username or Email is required")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}

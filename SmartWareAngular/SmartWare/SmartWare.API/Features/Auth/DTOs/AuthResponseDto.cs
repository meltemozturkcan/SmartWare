namespace SmartWare.API.Features.Auth.DTOs
{
    /// <summary>
    /// Login/Register başarılı olduğunda dönen response
    /// </summary>
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenExpiration { get; set; }
        public UserDto User { get; set; } = null!;
    }
}

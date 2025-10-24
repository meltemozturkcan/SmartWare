using SmartWare.API.Core.Common;

namespace SmartWare.API.Core.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }

        public string Role { get; set; } = "Reader";

        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}

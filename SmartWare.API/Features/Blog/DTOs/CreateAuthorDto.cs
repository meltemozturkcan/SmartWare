using System.ComponentModel.DataAnnotations;

namespace SmartWare.API.Features.Blog.DTOs
{
    /// <summary>
    /// Yeni yazar oluşturma için DTO
    /// </summary>
    public class CreateAuthorDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        public string Email { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string? Bio { get; set; }

        [Url(ErrorMessage = "Invalid avatar URL")]
        public string? AvatarUrl { get; set; }
    }
}

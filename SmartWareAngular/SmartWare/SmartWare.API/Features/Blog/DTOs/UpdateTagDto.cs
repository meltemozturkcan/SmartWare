using System.ComponentModel.DataAnnotations;

namespace SmartWare.API.Features.Blog.DTOs
{
    /// <summary>
    /// Tag güncelleme için DTO
    /// </summary>
    public class UpdateTagDto
    {
        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(60, ErrorMessage = "Slug cannot exceed 60 characters")]
        public string? Slug { get; set; }
    }
}

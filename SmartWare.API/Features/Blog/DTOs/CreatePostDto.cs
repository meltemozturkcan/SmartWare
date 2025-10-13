using System.ComponentModel.DataAnnotations;

namespace SmartWare.API.Features.Blog.DTOs
{
    public class CreatePostDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Slug cannot exceed 250 characters")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
        public string? Summary { get; set; }

        [Url(ErrorMessage = "Invalid image URL")]
        public string? FeaturedImageUrl { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        [Required(ErrorMessage = "AuthorId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid AuthorId")]
        public int AuthorId { get; set; }
        /// <summary>
        /// Post'a eklenecek tag ID'leri
        /// </summary>
        public List<int> TagIds { get; set; } = new();

    }
}
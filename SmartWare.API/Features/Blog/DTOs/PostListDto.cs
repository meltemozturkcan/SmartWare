namespace SmartWare.API.Features.Blog.DTOs
{
    public class PostListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public int ViewCount { get; set; }

        // Nested DTOs
        public AuthorDto Author { get; set; } = null!;
        public List<TagDto> Tags { get; set; } = new();
    }
}

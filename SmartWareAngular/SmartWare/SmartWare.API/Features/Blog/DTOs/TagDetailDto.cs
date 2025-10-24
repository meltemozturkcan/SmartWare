namespace SmartWare.API.Features.Blog.DTOs
{
    /// <summary>
    /// Tag detayı için DTO (postlarıyla birlikte)
    /// </summary>
    public class TagDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Bu tag'e sahip postlar
        public List<PostListDto> Posts { get; set; } = new();
    }
}

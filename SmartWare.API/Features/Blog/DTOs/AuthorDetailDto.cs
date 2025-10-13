namespace SmartWare.API.Features.Blog.DTOs
{
    public class AuthorDetailDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Yazarın postları
        public List<PostListDto> Posts { get; set; } = new();
    }
}
}

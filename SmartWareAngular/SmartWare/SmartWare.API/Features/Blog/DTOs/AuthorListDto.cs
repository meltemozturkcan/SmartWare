namespace SmartWare.API.Features.Blog.DTOs
{
    public class AuthorListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int PostCount { get; set; }  // Kaç post yazdı?
    }
}

using SmartWare.API.Core.Common;

namespace SmartWare.API.Core.Entities
{
    public class Post : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? FeaturedImageUrl { get; set; }

        public bool IsPublished { get; set; } = false;
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; } = 0;

        public int AuthorId { get; set; }

        // Navigation Properties - Şimdi ekle
        public Author Author { get; set; } = null!;
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public ICollection<PostView> PostViews { get; set; } = new List<PostView>();
    }
}

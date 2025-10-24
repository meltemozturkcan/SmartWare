using SmartWare.API.Core.Common;

namespace SmartWare.API.Core.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}

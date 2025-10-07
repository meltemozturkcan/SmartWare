using Microsoft.Extensions.Hosting;
using SmartWare.API.Core.Common;

namespace SmartWare.API.Core.Entities
{
    public class PostView : BaseEntity
    {
        public int PostId { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public Post Post { get; set; } = null!;
    }
}

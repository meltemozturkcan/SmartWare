using Azure;
using SmartWare.API.Core.Common;

namespace SmartWare.API.Core.Entities
{
    public class PostTag : BaseEntity
    {
        public int PostId { get; set; }
        public int TagId { get; set; }

        public Post Post { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}

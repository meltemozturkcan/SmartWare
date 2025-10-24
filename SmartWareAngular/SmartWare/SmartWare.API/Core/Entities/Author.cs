using Microsoft.Extensions.Hosting;
using SmartWare.API.Core.Common;
using System.Collections.Generic;

namespace SmartWare.API.Core.Entities
{
    
    public class Author : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }

        // Navigation Property
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}

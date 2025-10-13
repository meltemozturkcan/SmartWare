﻿namespace SmartWare.API.Features.Blog.DTOs
{
    /// <summary>
    /// Tag listesi için DTO
    /// </summary>
    public class TagListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int PostCount { get; set; }  // Kaç post'ta kullanıldı?
    }
}

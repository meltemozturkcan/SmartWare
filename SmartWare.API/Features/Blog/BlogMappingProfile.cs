using AutoMapper;
using SmartWare.API.Core.Entities;
using SmartWare.API.Features.Blog.DTOs;

namespace SmartWare.API.Features.Blog
{/// <summary>
 /// AutoMapper profile for Blog entities
 /// </summary>
    public class BlogMappingProfile : Profile
    {
        public BlogMappingProfile()
        {
            // ============================================
            // Author Mappings
            // ============================================
            CreateMap<Author, AuthorDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<Author, AuthorListDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.PostCount,
                    opt => opt.MapFrom(src => src.Posts.Count(p => p.IsPublished)));

            CreateMap<Author, AuthorDetailDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Posts,
                    opt => opt.MapFrom(src => src.Posts.Where(p => p.IsPublished)));

            CreateMap<CreateAuthorDto, Author>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore());

            CreateMap<UpdateAuthorDto, Author>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // ============================================
            // Tag Mappings
            // ============================================
            CreateMap<Tag, TagDto>();

            CreateMap<Tag, TagListDto>()
                .ForMember(dest => dest.PostCount,
                    opt => opt.MapFrom(src => src.PostTags.Count(pt => pt.Post.IsPublished)));

            CreateMap<Tag, TagDetailDto>()
                .ForMember(dest => dest.Posts,
                    opt => opt.MapFrom(src => src.PostTags
                        .Where(pt => pt.Post.IsPublished)
                        .Select(pt => pt.Post)));

            CreateMap<CreateTagDto, Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore());

            CreateMap<UpdateTagDto, Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // ============================================
            // Post → PostListDto (Liste için)
            // ============================================
            CreateMap<Post, PostListDto>()
                .ForMember(dest => dest.Author,
                    opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.PostTags.Select(pt => pt.Tag).ToList()));

            // ============================================
            // Post → PostDetailDto (Detay için)
            // ============================================
            CreateMap<Post, PostDetailDto>()
                .ForMember(dest => dest.Author,
                    opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.PostTags.Select(pt => pt.Tag).ToList()));

            // ============================================
            // CreatePostDto → Post (Yeni kayıt)
            // ============================================
            CreateMap<CreatePostDto, Post>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore())
                .ForMember(dest => dest.PostViews, opt => opt.Ignore())
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore());

            // ============================================
            // UpdatePostDto → Post (Güncelleme)
            // ============================================
            CreateMap<UpdatePostDto, Post>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore())
                .ForMember(dest => dest.PostViews, opt => opt.Ignore())
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}

// ============================================
// AÇIKLAMALAR
// ============================================

// 1. CreateMap<Source, Destination>:
//    - Kaynak tipten hedef tipe mapping
//    - AutoMapper otomatik eşleştirir (aynı isimli property'ler)

// 2. ForMember:
//    - Custom mapping kuralları
//    - Farklı isimli property'ler için
//    - Hesaplanmış değerler için

// 3. MapFrom:
//    - Kaynak değerden nasıl alınacağını belirtir
//    - Lambda expression ile
//    - Örnek: FirstName + LastName → FullName

// 4. Ignore:
//    - Bu property'yi mapping'e dahil etme
//    - ID, CreatedAt gibi sistem alanları
//    - Navigation property'ler

// 5. Tags Mapping:
//    - PostTags (junction) → Tags (direkt liste)
//    - Select ile dönüşüm
//    - Circular reference önler

// 6. UpdatedAt:
//    - Her update'te otomatik DateTime.UtcNow
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWare.API.Core.Entities;
using SmartWare.API.Data;
using SmartWare.API.Features.Blog.DTOs;

namespace SmartWare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<PostsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================
        // GET: api/posts
        // Tüm yayınlanmış postları getir (DTO ile)
        // ============================================
        [HttpGet]
        [ProducesResponseType(typeof(List<PostListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PostListDto>>> GetPosts()
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.IsPublished)
                    .OrderByDescending(p => p.PublishedAt)
                    .ToListAsync();

                var postsDto = _mapper.Map<List<PostListDto>>(posts);

                _logger.LogInformation("Retrieved {Count} published posts", postsDto.Count);

                return Ok(postsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // GET: api/posts/5
        // ID'ye göre tek post getir (DTO ile)
        // ============================================
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDetailDto>> GetPost(int id)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found", id);
                    return NotFound(new { message = $"Post with ID {id} not found" });
                }

                // View count artır
                post.ViewCount++;
                await _context.SaveChangesAsync();

                var postDto = _mapper.Map<PostDetailDto>(post);

                _logger.LogInformation("Retrieved post {PostId}: {Title}", id, post.Title);

                return Ok(postDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // GET: api/posts/slug/akilli-depo-sistemleri
        // Slug'a göre post getir (DTO ile)
        // ============================================
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDetailDto>> GetPostBySlug(string slug)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

                if (post == null)
                {
                    _logger.LogWarning("Post with slug '{Slug}' not found", slug);
                    return NotFound(new { message = $"Post with slug '{slug}' not found" });
                }

                post.ViewCount++;
                await _context.SaveChangesAsync();

                var postDto = _mapper.Map<PostDetailDto>(post);

                _logger.LogInformation("Retrieved post by slug '{Slug}': {Title}", slug, post.Title);

                return Ok(postDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post by slug '{Slug}'", slug);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // GET: api/posts/author/5
        // Yazarın tüm postlarını getir (DTO ile)
        // ============================================
        [HttpGet("author/{authorId}")]
        [ProducesResponseType(typeof(List<PostListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PostListDto>>> GetPostsByAuthor(int authorId)
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.AuthorId == authorId && p.IsPublished)
                    .OrderByDescending(p => p.PublishedAt)
                    .ToListAsync();

                var postsDto = _mapper.Map<List<PostListDto>>(posts);

                _logger.LogInformation("Retrieved {Count} posts for author {AuthorId}", postsDto.Count, authorId);

                return Ok(postsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for author {AuthorId}", authorId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // GET: api/posts/tag/akilli-depo
        // Tag'e göre postları getir (DTO ile)
        // ============================================
        [HttpGet("tag/{tagSlug}")]
        [ProducesResponseType(typeof(List<PostListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PostListDto>>> GetPostsByTag(string tagSlug)
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.PostTags.Any(pt => pt.Tag.Slug == tagSlug) && p.IsPublished)
                    .OrderByDescending(p => p.PublishedAt)
                    .ToListAsync();

                var postsDto = _mapper.Map<List<PostListDto>>(posts);

                _logger.LogInformation("Retrieved {Count} posts for tag '{TagSlug}'", postsDto.Count, tagSlug);

                return Ok(postsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for tag '{TagSlug}'", tagSlug);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // GET: api/posts/search?query=depo
        // Arama yap (DTO ile)
        // ============================================
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<PostListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PostListDto>>> SearchPosts([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query cannot be empty" });
                }

                var posts = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.IsPublished &&
                               (p.Title.Contains(query) ||
                                p.Content.Contains(query) ||
                                (p.Summary != null && p.Summary.Contains(query))))
                    .OrderByDescending(p => p.PublishedAt)
                    .ToListAsync();

                var postsDto = _mapper.Map<List<PostListDto>>(posts);

                _logger.LogInformation("Search for '{Query}' returned {Count} results", query, postsDto.Count);

                return Ok(postsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching posts with query '{Query}'", query);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // POST: api/posts
        // Yeni post oluştur (DTO ile)
        // ============================================
        [HttpPost]
        [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PostDetailDto>> CreatePost(CreatePostDto createPostDto)
        {
            try
            {
                // CreatePostDto → Post mapping
                var post = _mapper.Map<Post>(createPostDto);

                // Slug oluştur (eğer yoksa)
                if (string.IsNullOrWhiteSpace(post.Slug))
                {
                    post.Slug = GenerateSlug(post.Title);
                }

                // Slug unique mi kontrol et
                var existingPost = await _context.Posts
                    .FirstOrDefaultAsync(p => p.Slug == post.Slug);

                if (existingPost != null)
                {
                    return BadRequest(new { message = "A post with this slug already exists" });
                }

                // Tag ilişkilerini oluştur
                if (createPostDto.TagIds.Any())
                {
                    foreach (var tagId in createPostDto.TagIds)
                    {
                        post.PostTags.Add(new PostTag
                        {
                            TagId = tagId
                        });
                    }
                }

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                // Post'u yeniden çek (Author ve Tag'lerle birlikte)
                var createdPost = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstAsync(p => p.Id == post.Id);

                var postDto = _mapper.Map<PostDetailDto>(createdPost);

                _logger.LogInformation("Created new post {PostId}: {Title}", post.Id, post.Title);

                return CreatedAtAction(
                    nameof(GetPost),
                    new { id = post.Id },
                    postDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // PUT: api/posts/5
        // Post güncelle (DTO ile)
        // ============================================
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePost(int id, UpdatePostDto updatePostDto)
        {
            try
            {
                var existingPost = await _context.Posts
                    .Include(p => p.PostTags)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (existingPost == null)
                {
                    return NotFound(new { message = $"Post with ID {id} not found" });
                }

                // UpdatePostDto → Post mapping (sadece güncellenecek alanlar)
                _mapper.Map(updatePostDto, existingPost);

                // Slug oluştur (eğer yoksa)
                if (string.IsNullOrWhiteSpace(existingPost.Slug))
                {
                    existingPost.Slug = GenerateSlug(existingPost.Title);
                }

                // Tag ilişkilerini güncelle
                existingPost.PostTags.Clear();
                if (updatePostDto.TagIds.Any())
                {
                    foreach (var tagId in updatePostDto.TagIds)
                    {
                        existingPost.PostTags.Add(new PostTag
                        {
                            PostId = id,
                            TagId = tagId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated post {PostId}: {Title}", id, existingPost.Title);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // DELETE: api/posts/5
        // Post sil (Soft Delete)
        // ============================================
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);

                if (post == null)
                {
                    return NotFound(new { message = $"Post with ID {id} not found" });
                }

                post.IsDeleted = true;
                post.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted post {PostId}: {Title}", id, post.Title);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================
        private string GenerateSlug(string title)
        {
            var slug = title.ToLowerInvariant()
                .Replace('ş', 's')
                .Replace('ğ', 'g')
                .Replace('ü', 'u')
                .Replace('ö', 'o')
                .Replace('ç', 'c')
                .Replace('ı', 'i')
                .Replace(' ', '-')
                .Replace("'", "");

            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}

// ============================================
// DEĞİŞİKLİKLER
// ============================================

// 1. IMapper Injection:
//    - AutoMapper servisini constructor'da al

// 2. Return Type Değişti:
//    - Post → PostListDto / PostDetailDto
//    - Güvenli, kontrollü response

// 3. _mapper.Map() Kullanımı:
//    - Entity → DTO dönüşümü otomatik
//    - Navigation property'ler dahil

// 4. CreatePostDto:
//    - Validation attributes çalışır
//    - [ApiController] otomatik validate eder
//    - BadRequest 400 otomatik döner

// 5. UpdatePostDto:
//    - Sadece güncellenebilir alanlar
//    - Id, CreatedAt korumalı

// 6. ProducesResponseType:
//    - Swagger dokümantasyonu için
//    - Response tipini belirtir
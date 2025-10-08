using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWare.API.Core.Entities;
using SmartWare.API.Data;

namespace SmartWare.API.Controllers
{
    /// <summary>
    /// Blog post yönetimi için API endpoint'leri
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostsController> _logger;

        public PostsController(ApplicationDbContext context, ILogger<PostsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // GET: api/posts
        // Tüm yayınlanmış postları getir
        // ============================================
        /// <summary>
        /// Tüm yayınlanmış blog postlarını listeler
        /// </summary>
        /// <returns>Post listesi</returns>
        /// <response code="200">Başarılı - Post listesi döner</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.Author)           // Author bilgisini de getir
                    .Include(p => p.PostTags)         // Tag ilişkilerini getir
                        .ThenInclude(pt => pt.Tag)    // Tag detaylarını getir
                    .Where(p => p.IsPublished)        // Sadece yayınlanmış postlar
                    .OrderByDescending(p => p.PublishedAt)  // En yeni önce
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} published posts", posts.Count);

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts");
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // GET: api/posts/5
        // ID'ye göre tek post getir
        // ============================================
        /// <summary>
        /// Belirli bir blog postunu ID'sine göre getirir
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Post detayı</returns>
        /// <response code="200">Başarılı - Post bulundu</response>
        /// <response code="404">Post bulunamadı</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Post>> GetPost(int id)
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

                _logger.LogInformation("Retrieved post {PostId}: {Title}", id, post.Title);

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post {PostId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // GET: api/posts/slug/akilli-depo-sistemleri
        // Slug'a göre post getir (SEO-friendly URL)
        // ============================================
        /// <summary>
        /// Blog postunu slug'ına göre getirir (SEO-friendly)
        /// </summary>
        /// <param name="slug">Post slug (URL-friendly)</param>
        /// <returns>Post detayı</returns>
        /// <response code="200">Başarılı - Post bulundu</response>
        /// <response code="404">Post bulunamadı</response>
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Post>> GetPostBySlug(string slug)
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

                // View count artır
                post.ViewCount++;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Retrieved post by slug '{Slug}': {Title}", slug, post.Title);

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post by slug '{Slug}'", slug);
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // GET: api/posts/author/5
        // Yazarın tüm postlarını getir
        // ============================================
        /// <summary>
        /// Belirli bir yazarın tüm blog postlarını listeler
        /// </summary>
        /// <param name="authorId">Yazar ID</param>
        /// <returns>Yazarın post listesi</returns>
        /// <response code="200">Başarılı</response>
        [HttpGet("author/{authorId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByAuthor(int authorId)
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

                _logger.LogInformation("Retrieved {Count} posts for author {AuthorId}", posts.Count, authorId);

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for author {AuthorId}", authorId);
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // GET: api/posts/tag/akilli-depo
        // Tag'e göre postları getir
        // ============================================
        /// <summary>
        /// Belirli bir etikete sahip blog postlarını listeler
        /// </summary>
        /// <param name="tagSlug">Tag slug</param>
        /// <returns>Tag'e sahip post listesi</returns>
        /// <response code="200">Başarılı</response>
        [HttpGet("tag/{tagSlug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByTag(string tagSlug)
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

                _logger.LogInformation("Retrieved {Count} posts for tag '{TagSlug}'", posts.Count, tagSlug);

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for tag '{TagSlug}'", tagSlug);
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // GET: api/posts/search?query=depo
        // Arama yap
        // ============================================
        /// <summary>
        /// Blog postlarında arama yapar (title, content, summary)
        /// </summary>
        /// <param name="query">Arama terimi</param>
        /// <returns>Arama sonuçları</returns>
        /// <response code="200">Başarılı</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Post>>> SearchPosts([FromQuery] string query)
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

                _logger.LogInformation("Search for '{Query}' returned {Count} results", query, posts.Count);

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching posts with query '{Query}'", query);
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // POST: api/posts
        // Yeni post oluştur
        // ============================================
        /// <summary>
        /// Yeni bir blog postu oluşturur
        /// </summary>
        /// <param name="post">Post verisi</param>
        /// <returns>Oluşturulan post</returns>
        /// <response code="201">Başarılı - Post oluşturuldu</response>
        /// <response code="400">Geçersiz veri</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(post.Title))
                {
                    return BadRequest(new { message = "Title is required" });
                }

                // Slug oluştur (basit versiyon)
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

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new post {PostId}: {Title}", post.Id, post.Title);

                return CreatedAtAction(
                    nameof(GetPost),
                    new { id = post.Id },
                    post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // PUT: api/posts/5
        // Post güncelle
        // ============================================
        /// <summary>
        /// Mevcut bir blog postunu günceller
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="post">Güncellenmiş post verisi</param>
        /// <returns>Sonuç</returns>
        /// <response code="204">Başarılı - Post güncellendi</response>
        /// <response code="400">Geçersiz veri</response>
        /// <response code="404">Post bulunamadı</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePost(int id, Post post)
        {
            try
            {
                if (id != post.Id)
                {
                    return BadRequest(new { message = "ID mismatch" });
                }

                var existingPost = await _context.Posts.FindAsync(id);

                if (existingPost == null)
                {
                    return NotFound(new { message = $"Post with ID {id} not found" });
                }

                // Update properties
                existingPost.Title = post.Title;
                existingPost.Slug = post.Slug;
                existingPost.Content = post.Content;
                existingPost.Summary = post.Summary;
                existingPost.FeaturedImageUrl = post.FeaturedImageUrl;
                existingPost.IsPublished = post.IsPublished;
                existingPost.PublishedAt = post.PublishedAt;
                existingPost.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated post {PostId}: {Title}", id, post.Title);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // DELETE: api/posts/5
        // Post sil (Soft Delete)
        // ============================================
        /// <summary>
        /// Bir blog postunu siler (soft delete)
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Sonuç</returns>
        /// <response code="204">Başarılı - Post silindi</response>
        /// <response code="404">Post bulunamadı</response>
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

                // Soft delete
                post.IsDeleted = true;
                post.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted post {PostId}: {Title}", id, post.Title);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        /// <summary>
        /// Başlıktan slug oluşturur (basit versiyon)
        /// </summary>
        private string GenerateSlug(string title)
        {
            // Türkçe karakterleri değiştir
            var slug = title.ToLowerInvariant()
                .Replace('ş', 's')
                .Replace('ğ', 'g')
                .Replace('ü', 'u')
                .Replace('ö', 'o')
                .Replace('ç', 'c')
                .Replace('ı', 'i')
                .Replace(' ', '-')
                .Replace("'", "");

            // Özel karakterleri temizle
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Birden fazla tire varsa tek tireye düşür
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

            // Baştaki ve sondaki tireleri kaldır
            slug = slug.Trim('-');

            return slug;
        }
    }
}

// ============================================
// AÇIKLAMALAR
// ============================================

// 1. [ApiController] Attribute:
//    - Otomatik model validation
//    - Otomatik 400 BadRequest response
//    - [FromBody] otomatik binding

// 2. [Route("api/[controller]")]:
//    - /api/posts URL'i oluşturur
//    - [controller] → PostsController'dan "Posts" alır

// 3. async/await:
//    - Database işlemleri asenkron
//    - Performans artışı
//    - Non-blocking operations

// 4. Include():
//    - Eager loading
//    - İlişkili entity'leri de getirir
//    - N+1 problem'ini önler

// 5. Status Codes:
//    - 200 OK: Başarılı
//    - 201 Created: Yeni kayıt oluşturuldu
//    - 204 No Content: İşlem başarılı, response body yok
//    - 400 Bad Request: Geçersiz istek
//    - 404 Not Found: Kayıt bulunamadı
//    - 500 Internal Server Error: Sunucu hatası

// 6. Logging:
//    - ILogger ile loglama
//    - Hata takibi
//    - Audit trail

// 7. Soft Delete:
//    - Fiziksel silme yok
//    - IsDeleted = true
//    - Query filter otomatik filtreler

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartWare.API.Core.Entities;
using SmartWare.API.Data;
using SmartWare.API.Features.Blog.DTOs;

namespace SmartWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AuthorsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================
        // GET: api/authors
        // Tüm yazarları listele
        // ============================================
        [HttpGet]
        [ProducesResponseType(typeof(List<AuthorListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AuthorListDto>>> GetAuthors()
        {
            try
            {
                var authors = await _context.Authors
                    .Include(a => a.Posts.Where(p => p.IsPublished))
                    .ToListAsync();

                var authorsDto = _mapper.Map<List<AuthorListDto>>(authors);

                _logger.LogInformation("Retrieved {Count} authors", authorsDto.Count);

                return Ok(authorsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving authors");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // GET: api/authors/5
        // ID'ye göre yazar getir (postlarıyla birlikte)
        // ============================================
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuthorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorDetailDto>> GetAuthor(int id)
        {
            try
            {
                var author = await _context.Authors
                    .Include(a => a.Posts.Where(p => p.IsPublished))
                        .ThenInclude(p => p.PostTags)
                            .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (author == null)
                {
                    _logger.LogWarning("Author with ID {AuthorId} not found", id);
                    return NotFound(new { message = $"Author with ID {id} not found" });
                }

                var authorDto = _mapper.Map<AuthorDetailDto>(author);

                _logger.LogInformation("Retrieved author {AuthorId}: {Name}", id, authorDto.FullName);

                return Ok(authorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving author {AuthorId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // POST: api/authors
        // Yeni yazar oluştur
        // ============================================
        [HttpPost]
        [ProducesResponseType(typeof(AuthorDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthorDetailDto>> CreateAuthor(CreateAuthorDto createAuthorDto)
        {
            try
            {
                // Email unique mi kontrol et
                var existingAuthor = await _context.Authors
                    .FirstOrDefaultAsync(a => a.Email == createAuthorDto.Email);

                if (existingAuthor != null)
                {
                    return BadRequest(new { message = "An author with this email already exists" });
                }

                var author = _mapper.Map<Author>(createAuthorDto);

                _context.Authors.Add(author);
                await _context.SaveChangesAsync();

                var authorDto = _mapper.Map<AuthorDetailDto>(author);

                _logger.LogInformation("Created new author {AuthorId}: {Name}", author.Id, authorDto.FullName);

                return CreatedAtAction(
                    nameof(GetAuthor),
                    new { id = author.Id },
                    authorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating author");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // PUT: api/authors/5
        // Yazar güncelle
        // ============================================
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(int id, UpdateAuthorDto updateAuthorDto)
        {
            try
            {
                var existingAuthor = await _context.Authors.FindAsync(id);

                if (existingAuthor == null)
                {
                    return NotFound(new { message = $"Author with ID {id} not found" });
                }

                // Email unique mi kontrol et (başka yazar kullanıyor mu?)
                var authorWithSameEmail = await _context.Authors
                    .FirstOrDefaultAsync(a => a.Email == updateAuthorDto.Email && a.Id != id);

                if (authorWithSameEmail != null)
                {
                    return BadRequest(new { message = "Another author with this email already exists" });
                }

                _mapper.Map(updateAuthorDto, existingAuthor);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated author {AuthorId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating author {AuthorId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // DELETE: api/authors/5
        // Yazar sil (Soft Delete)
        // ============================================
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            try
            {
                var author = await _context.Authors
                    .Include(a => a.Posts)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (author == null)
                {
                    return NotFound(new { message = $"Author with ID {id} not found" });
                }

                // Yayınlanmış postları var mı kontrol et
                if (author.Posts.Any(p => p.IsPublished))
                {
                    return BadRequest(new { message = "Cannot delete author with published posts" });
                }

                author.IsDeleted = true;
                author.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted author {AuthorId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting author {AuthorId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

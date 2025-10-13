using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWare.API.Core.Entities;
using SmartWare.API.Data;
using SmartWare.API.Features.Blog.DTOs;

namespace SmartWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TagsController> _logger;

        public TagsController(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<TagsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================
        // GET: api/tags
        // Tüm etiketleri listele
        // ============================================
        [HttpGet]
        [ProducesResponseType(typeof(List<TagListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TagListDto>>> GetTags()
        {
            try
            {
                var tags = await _context.Tags
                    .Include(t => t.PostTags.Where(pt => pt.Post.IsPublished))
                    .ToListAsync();

                var tagsDto = _mapper.Map<List<TagListDto>>(tags);

                _logger.LogInformation("Retrieved {Count} tags", tagsDto.Count);

                return Ok(tagsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tags");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // GET: api/tags/5
        // ID'ye göre tag getir (postlarıyla birlikte)
        // ============================================
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TagDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagDetailDto>> GetTag(int id)
        {
            try
            {
                var tag = await _context.Tags
                    .Include(t => t.PostTags.Where(pt => pt.Post.IsPublished))
                        .ThenInclude(pt => pt.Post)
                            .ThenInclude(p => p.Author)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tag == null)
                {
                    _logger.LogWarning("Tag with ID {TagId} not found", id);
                    return NotFound(new { message = $"Tag with ID {id} not found" });
                }

                var tagDto = _mapper.Map<TagDetailDto>(tag);

                _logger.LogInformation("Retrieved tag {TagId}: {Name}", id, tag.Name);

                return Ok(tagDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag {TagId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // POST: api/tags
        // Yeni tag oluştur
        // ============================================
        [HttpPost]
        [ProducesResponseType(typeof(TagDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TagDetailDto>> CreateTag(CreateTagDto createTagDto)
        {
            try
            {
                var tag = _mapper.Map<Tag>(createTagDto);

                // Slug oluştur (eğer yoksa)
                if (string.IsNullOrWhiteSpace(tag.Slug))
                {
                    tag.Slug = GenerateSlug(tag.Name);
                }

                // Slug unique mi kontrol et
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Slug == tag.Slug);

                if (existingTag != null)
                {
                    return BadRequest(new { message = "A tag with this slug already exists" });
                }

                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

                var tagDto = _mapper.Map<TagDetailDto>(tag);

                _logger.LogInformation("Created new tag {TagId}: {Name}", tag.Id, tag.Name);

                return CreatedAtAction(
                    nameof(GetTag),
                    new { id = tag.Id },
                    tagDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // PUT: api/tags/5
        // Tag güncelle
        // ============================================
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTag(int id, UpdateTagDto updateTagDto)
        {
            try
            {
                var existingTag = await _context.Tags.FindAsync(id);

                if (existingTag == null)
                {
                    return NotFound(new { message = $"Tag with ID {id} not found" });
                }

                _mapper.Map(updateTagDto, existingTag);

                // Slug oluştur (eğer yoksa)
                if (string.IsNullOrWhiteSpace(existingTag.Slug))
                {
                    existingTag.Slug = GenerateSlug(existingTag.Name);
                }

                // Slug unique mi kontrol et (başka tag kullanıyor mu?)
                var tagWithSameSlug = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Slug == existingTag.Slug && t.Id != id);

                if (tagWithSameSlug != null)
                {
                    return BadRequest(new { message = "Another tag with this slug already exists" });
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated tag {TagId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag {TagId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // DELETE: api/tags/5
        // Tag sil (Soft Delete)
        // ============================================
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteTag(int id)
        {
            try
            {
                var tag = await _context.Tags
                    .Include(t => t.PostTags)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tag == null)
                {
                    return NotFound(new { message = $"Tag with ID {id} not found" });
                }

                // Kullanımda mı kontrol et
                if (tag.PostTags.Any())
                {
                    return BadRequest(new { message = "Cannot delete tag that is in use" });
                }

                tag.IsDeleted = true;
                tag.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted tag {TagId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag {TagId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================
        private string GenerateSlug(string name)
        {
            var slug = name.ToLowerInvariant()
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

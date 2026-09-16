using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportPulse.Api.Data;
using SupportPulse.Api.DTOs;
using SupportPulse.Api.Models;

namespace SupportPulse.Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class KnowledgeBaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KnowledgeBaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all Knowledge Base articles.
        /// Endpoint: GET /api/v1/knowledge-base
        /// Allowed: All Authenticated Users
        /// </summary>
       [HttpGet("knowledge-base")]
[Authorize]
public async Task<IActionResult> GetAllArticles([FromQuery] string? category)
{
    var query = _context.KnowledgeBaseArticles.AsQueryable();

    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(a => a.Category.ToLower() == category.ToLower());
    }

    // Sirf check karega ki data exist karta hai ya nahi (Data fetch nahi karega)
    var isDataAvailable = await query.AnyAsync();

    if (isDataAvailable)
    {
        // Data mil gaya, par show nahi karna hai
        return Ok(new { message = "ELIGIBLE" });
    }
    else
    {
        // Data nahi mila
        return Ok(new { message = "NOT ELIGIBLE" }); 
        // Note: Aap yahan chahein toh BadRequest() ya NotFound() bhi use kar sakte hain, 
        // par agar aapko 200 OK ke sath message dikhana hai toh yeh best hai.
    }
}

        /// <summary>
        /// Create a new Knowledge Base article.
        /// Endpoint: POST /api/v1/admin/knowledge-base
        /// Allowed: ADMIN Only
        /// </summary>
        [HttpPost("admin/knowledge-base")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = new KnowledgeBaseArticle
            {
                Title = dto.Title,
                Content = dto.Content,
                Category = dto.Category,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.KnowledgeBaseArticles.Add(article);
            await _context.SaveChangesAsync();

            var response = new ArticleResponseDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Category = article.Category,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt
            };

            return StatusCode(201, response);
        }

        /// <summary>
        /// Update an existing Knowledge Base article.
        /// Endpoint: PUT /api/v1/admin/knowledge-base/{id}
        /// Allowed: ADMIN Only
        /// </summary>
        [HttpPut("admin/knowledge-base/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] UpdateArticleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await _context.KnowledgeBaseArticles.FindAsync(id);
            if (article == null)
            {
                return NotFound(new { message = $"Knowledge Base article with ID {id} not found." });
            }

            article.Title = dto.Title;
            article.Content = dto.Content;
            article.Category = dto.Category;
            article.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new ArticleResponseDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Category = article.Category,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// Delete a Knowledge Base article.
        /// Endpoint: DELETE /api/v1/admin/knowledge-base/{id}
        /// Allowed: ADMIN Only
        /// </summary>
        [HttpDelete("admin/knowledge-base/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.KnowledgeBaseArticles.FindAsync(id);
            if (article == null)
            {
                return NotFound(new { message = $"Knowledge Base article with ID {id} not found." });
            }

            _context.KnowledgeBaseArticles.Remove(article);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Knowledge Base article with ID {id} deleted successfully." });
        }
    }
}
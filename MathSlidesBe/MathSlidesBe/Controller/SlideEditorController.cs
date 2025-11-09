using System.Security.Claims;
using MathSlidesBe.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathSlidesBe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlideEditorController : ControllerBase
    {
        private readonly MathSlidesDbContext _context;

        public SlideEditorController(MathSlidesDbContext context)
        {
            _context = context;
        }

        [HttpPut("presentation")]
        public async Task<IActionResult> UpdatePresentationTitle([FromBody] UpdatePresentationTitleRequest request)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null)
            {
                return Unauthorized();
            }
            if (!Guid.TryParse(currentUserId, out var userId))
            {
                return Unauthorized();
            }

            var presentation = await _context.Presentations.FirstOrDefaultAsync(p => p.Id == request.PresentationId);
            if (presentation == null)
            {
                return NotFound();
            }
            if (presentation.UserID != userId)
            {
                return Forbid();
            }

            presentation.Title = request.NewTitle;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Presentation title updated successfully." });
        }
    }
}

using System.Security.Claims;
using MathSlidesBe.BaseRepo;
using MathSlidesBe.Entity;
using MathSlidesBe.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MathSlidesBe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlideEditorController : ControllerBase
    {
        private readonly IRepository<Presentation> _presentationRepository;

        public SlideEditorController(IRepository<Presentation> presentationRepository)
        {
            _presentationRepository = presentationRepository;
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

            var presentation = await _presentationRepository.GetByIdAsync(request.PresentationId);
            if (presentation == null)
            {
                return NotFound();
            }
            if (presentation.UserID != userId)
            {
                return Forbid();
            }

            presentation.Title = request.NewTitle;
            await _presentationRepository.UpdateAsync(presentation);

            return Ok(new { message = "Presentation title updated successfully." });
        }
    }
}
using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace MathSlidesBe.Controllers
{
    // ViewModels to match frontend structure
    public class ComponentPropertiesViewModel
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Content { get; set; }
        public double Rotation { get; set; }
        public double FontSize { get; set; }
        public string Color { get; set; }
        public string BackgroundColor { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
    }

    public class ComponentViewModel
    {
        public Guid? Id { get; set; }
        public Guid? SlideID { get; set; }
        public string ComponentType { get; set; }
        public ComponentPropertiesViewModel? Properties { get; set; }
        public int ZIndex { get; set; }
    }

    public class SlideViewModel
    {
        public Guid? Id { get; set; }
        public Guid? PresentationID { get; set; }
        public int PageNumber { get; set; }
        public List<ComponentViewModel>? Components { get; set; }
    }

    public class PresentationViewModel
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public Guid UserID { get; set; }
        public Guid LessonID { get; set; }
        public List<SlideViewModel>? Slides { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class PresentationController : ControllerBase
    {
        private readonly IRepository<Presentation> _repository;
        private readonly MathSlidesDbContext _context;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true
        };

        public PresentationController(IRepository<Presentation> repository, MathSlidesDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<PresentationViewModel>>> GetById(Guid id)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var userId))
            {
                return Unauthorized(BaseResponse<PresentationViewModel>.Fail("User is not authenticated."));
            }

            var presentation = await _context.Presentations.Where(p => !p.IsDeleted)
                                     .Include(p => p.Slides)
                                     .ThenInclude(s => s.Components)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(p => p.Id == id);

            if (presentation == null)
            {
                return NotFound(BaseResponse<PresentationViewModel>.Fail($"Presentation with id {id} not found."));
            }

            if (presentation.UserID != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, BaseResponse<PresentationViewModel>.Fail("User is not authorized to view this presentation."));
            }

            var presentationViewModel = new PresentationViewModel
            {
                Id = presentation.Id,
                Title = presentation.Title,
                UserID = presentation.UserID,
                LessonID = presentation.LessonID,
                Slides = presentation.Slides.Select(s => new SlideViewModel
                {
                    Id = s.Id,
                    PresentationID = s.PresentationID,
                    PageNumber = s.PageNumber,
                    Components = s.Components.Select(c => new ComponentViewModel
                    {
                        Id = c.Id,
                        SlideID = c.SlideID,
                        ComponentType = c.ComponentType,
                        Properties = JsonSerializer.Deserialize<ComponentPropertiesViewModel>(c.Properties, _jsonOptions),
                        ZIndex = c.ZIndex
                    }).ToList()
                }).ToList()
            };

            return Ok(BaseResponse<PresentationViewModel>.Ok(presentationViewModel, "Presentation retrieved successfully."));
        }

        [HttpGet("show-slide/{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<PresentationViewModel>>> GetShowSlide(Guid id)
        {
            var presentation = await _context.Presentations.Where(p => !p.IsDeleted)
                                     .Include(p => p.Slides)
                                     .ThenInclude(s => s.Components)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(p => p.Id == id);

            if (presentation == null)
            {
                return NotFound(BaseResponse<PresentationViewModel>.Fail($"Presentation with id {id} not found."));
            }

            var presentationViewModel = new PresentationViewModel
            {
                Id = presentation.Id,
                Title = presentation.Title,
                UserID = presentation.UserID,
                LessonID = presentation.LessonID,
                Slides = presentation.Slides.Select(s => new SlideViewModel
                {
                    Id = s.Id,
                    PresentationID = s.PresentationID,
                    PageNumber = s.PageNumber,
                    Components = s.Components.Select(c => new ComponentViewModel
                    {
                        Id = c.Id,
                        SlideID = c.SlideID,
                        ComponentType = c.ComponentType,
                        Properties = JsonSerializer.Deserialize<ComponentPropertiesViewModel>(c.Properties, _jsonOptions),
                        ZIndex = c.ZIndex
                    }).ToList()
                }).ToList()
            };

            return Ok(BaseResponse<PresentationViewModel>.Ok(presentationViewModel, "Presentation retrieved successfully."));
        }


        [HttpPost]
        public async Task<ActionResult<BaseResponse<PresentationViewModel>>> Create([FromBody] PresentationCreateDto dto)
        {
            var lessonId = dto.lessonId;
            var title = dto.title;
            if (lessonId == Guid.Empty)
            {
                return BadRequest(BaseResponse<PresentationViewModel>.Fail("Invalid lessonId"));
            }

            var presentation = new Presentation
            {
                Title = string.IsNullOrWhiteSpace(title) ? "New Presentation" : title,
                LessonID = lessonId
            };
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var userId))
            {
                return Unauthorized(BaseResponse<PresentationViewModel>.Fail("User is not authenticated."));
            }
            presentation.UserID = userId;
            var createdPresentation = await _repository.AddAsync(presentation);

            var viewModel = new PresentationViewModel
            {
                Id = createdPresentation.Id,
                Title = createdPresentation.Title,
                UserID = createdPresentation.UserID,
                LessonID = createdPresentation.LessonID,
                Slides = new List<SlideViewModel>() // A new presentation has no slides
            };

            var response = BaseResponse<PresentationViewModel>.Ok(viewModel, "Presentation created successfully.");

            return CreatedAtAction(nameof(GetById), new { id = viewModel.Id.Value }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PresentationViewModel dto)
        {
            if (dto.Id == null || id != dto.Id.Value)
            {
                return BadRequest(new { message = "Route ID and presentation ID in body do not match." });
            }

            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var userId))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var presentation = await _context.Presentations
                                             .Include(p => p.Slides)
                                             .ThenInclude(s => s.Components)
                                             .FirstOrDefaultAsync(p => p.Id == id);

                    if (presentation == null)
                    {
                        return NotFound(new { message = $"Presentation with id {id} not found." });
                    }

                    if (presentation.UserID != userId)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new { message = "User is not authorized to update this presentation." });
                    }

                    // Update scalar properties
                    presentation.Title = dto.Title;

                    // Remove old child entities using the change tracker
                    _context.Slides.RemoveRange(presentation.Slides);

                    // First save: commit the deletions and the title update
                    await _context.SaveChangesAsync();

                    if (dto.Slides != null && dto.Slides.Any())
                    {
                        var newSlides = dto.Slides.Select(sDto =>
                        {
                            var newSlide = new Slide
                            {
                                Id = Guid.NewGuid(),
                                PresentationID = presentation.Id,
                                PageNumber = sDto.PageNumber,
                            };

                            if (sDto.Components != null)
                            {
                                newSlide.Components = sDto.Components.Select(cDto => new Component
                                {
                                    Id = Guid.NewGuid(),
                                    SlideID = newSlide.Id,
                                    ComponentType = cDto.ComponentType,
                                    ZIndex = cDto.ZIndex,
                                    Properties = JsonSerializer.Serialize(cDto.Properties, _jsonOptions)
                                }).ToList();
                            }

                            return newSlide;
                        }).ToList();
                        await _context.Slides.AddRangeAsync(newSlides);
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status409Conflict, new { message = "The data was modified by another user. Please reload and try again." });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    //Log the exception
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during the update." });
                }
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var userId))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            var presentation = await _context.Presentations.FindAsync(id);
            if (presentation == null)
            {
                return NotFound(new { message = $"Presentation with id {id} not found." });
            }

            if (presentation.UserID != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "User is not authorized to delete this presentation." });
            }

            await _repository.DeleteAsync(id);

            return NoContent();
        }
    }
}

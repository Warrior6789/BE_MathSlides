using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Models.Dto;
using MathSlidesBe.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace MathSlidesBe.Controllers
{
        [Route("api/[controller]")]
        [ApiController]
        public class PresentationController : ControllerBase
        {
            private readonly IRepository<Presentation> _presentationRepository;
            private readonly IRepository<Slide> _slideRepository;
            private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                AllowTrailingCommas = true
            };

            public PresentationController(IRepository<Presentation> PresentationRepository, IRepository<Slide> _SlideRepository)
            {
                _presentationRepository = PresentationRepository;
                _slideRepository = _SlideRepository;
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

                var presentation = await _presentationRepository.GetByIdWithIncludesAsync(
                    id,
                    p => p.Slides,
                    p => p.Slides.SelectMany(s => s.Components)
                    );

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
                var presentation = await _presentationRepository.GetByIdWithIncludesAsync(
                        id,
                         p => p.Slides,
                        p => p.Slides.SelectMany(s => s.Components)
                    );

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
            [Authorize]
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
                var createdPresentation = await _presentationRepository.AddAsync(presentation);

                var viewModel = new PresentationViewModel
                {
                    Id = createdPresentation.Id,
                    Title = createdPresentation.Title,
                    UserID = createdPresentation.UserID,
                    LessonID = createdPresentation.LessonID,
                    Slides = new List<SlideViewModel>()
                };

                var response = BaseResponse<PresentationViewModel>.Ok(viewModel, "Presentation created successfully.");

                return CreatedAtAction(nameof(GetById), new { id = viewModel.Id.Value }, response);
            }

            [HttpPut("{id:guid}")]
            [Authorize]
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

                try
                {
                    await _presentationRepository.ExcuteInTransactionAsync(async () =>
                    {
                        var presentation = await _presentationRepository.GetByIdWithIncludesAsync(id, p => p.Slides, p => p.Slides.SelectMany(s => s.Components)
                        );
                        if (presentation == null)
                        {
                            throw new KeyNotFoundException();
                        }
                        if (presentation.UserID != userId)
                        {
                            throw new UnauthorizedAccessException();
                        }
                        presentation.Title = dto.Title;

                        await _slideRepository.RemoveRangeAsync(presentation.Slides);

                        await _presentationRepository.SaveChangeAsync();
                        if (dto.Slides != null && dto.Slides.Any())
                        {
                            var newSlides = dto.Slides.Select(sdto =>
                            {
                                var newSlideId = Guid.NewGuid();

                                var newSlide = new Slide
                                {
                                    Id = newSlideId,
                                    PresentationID = presentation.Id,
                                    PageNumber = sdto.PageNumber,
                                };

                                if (sdto.Components != null && sdto.Components.Any())
                                {
                                    newSlide.Components = sdto.Components.Select(cdto => new Component
                                    {
                                        Id = Guid.NewGuid(),
                                        SlideID = newSlideId,  
                                        ComponentType = cdto.ComponentType,
                                        Properties = JsonSerializer.Serialize(cdto.Properties, _jsonOptions),
                                        ZIndex = cdto.ZIndex
                                    }).ToList();
                                }

                                return newSlide;

                            }).ToList();

                            await _slideRepository.AddRangeAsync(newSlides);
                            await _presentationRepository.SaveChangeAsync();
                        }

                        return true;
                    }
                    );

                    return NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { Message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return StatusCode(StatusCodes.Status409Conflict, new
                    {
                        message = "The data was modified by another user. Please reload and try again."
                    });
                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An error occurred during the update."
                    });
                }
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

                var presentation = await _presentationRepository.FindAsync(id);
                if (presentation == null)
                {
                    return NotFound(new { message = $"Presentation with id {id} not found." });
                }

                if (presentation.UserID != userId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "User is not authorized to delete this presentation." });
                }

                await _presentationRepository.DeleteAsync(id);

                return NoContent();
            }
        }
    }


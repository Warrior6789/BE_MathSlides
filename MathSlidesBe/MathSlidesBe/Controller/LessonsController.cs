using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Models.Dto;
using MathSlidesBe.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MathSlidesBe.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly IRepository<Lesson> _repository;
        private readonly IRepository<Chapter> _chapterRepository;
        private readonly IRepository<Grade> _graderRepository;
        public LessonsController(IRepository<Lesson> repository,
            IRepository<Chapter> chapterRepository, IRepository<Grade> graderRepository)
        {
            _repository = repository;
            _chapterRepository = chapterRepository;
            _graderRepository = graderRepository;
        }

        // GET: api/lessons
        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<Lesson>>>> GetAll()
        {
            var lessons = await _repository.GetAllAsync();
            return Ok(BaseResponse<IEnumerable<Lesson>>.Ok(lessons, "Lấy danh sách thành công"));
        }
        [AllowAnonymous]
        [HttpGet("paged")]
        public async Task<ActionResult<BaseResponse<PagedResult<LessonViewModel>>>> GetPaged(
            int pageIndex = 1,
            int pageSize = 10,
            [FromQuery] Guid? chapterId = null,
            [FromQuery] Guid? gradeId = null,
            [FromQuery] string? keyword = null)
        {
            keyword = keyword?.Trim().ToLower();

            var query = _repository.Query(l =>
                !l.IsDeleted &&
                (!chapterId.HasValue || l.ChapterID == chapterId.Value) &&
                (!gradeId.HasValue || (l.Chapter != null && l.Chapter.GradeID == gradeId.Value)) &&
                (string.IsNullOrEmpty(keyword) || l.LessonName.ToLower().Contains(keyword))
            )
            .Include(l => l.Presentations)
            .Include(l => l.Chapter)
                .ThenInclude(c => c.Grade);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(l => l.LessonName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(lesson => new LessonViewModel
                {
                    Id = lesson.Id,
                    LessonName = lesson.LessonName,
                    Requirements = lesson.Requirements,
                    ChapterID = lesson.ChapterID,
                    GradeName = lesson.Chapter != null && lesson.Chapter.Grade != null
                                ? lesson.Chapter.Grade.GradeName
                                : string.Empty,
                    PresentationCount = lesson.Presentations.Count(),
                    FirstPresentationId = lesson.Presentations.Select(p => p.Id).FirstOrDefault()
                })
                .ToListAsync();

            var pagedResult = new PagedResult<LessonViewModel>
            {
                Items = items,
                TotalItems = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Ok(BaseResponse<PagedResult<LessonViewModel>>.Ok(
                pagedResult,
                "Lấy danh sách bài học phân trang, lọc và tìm kiếm thành công"
            ));
        }

        // GET: api/lessons/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BaseResponse<LessonViewModel>>> GetById(Guid id)
        {
            var lesson = await _repository.Query(l => l.Id == id && !l.IsDeleted)
                .Include(l => l.Chapter)
                    .ThenInclude(c => c.Grade)
                .Include(l => l.Presentations)
                .FirstOrDefaultAsync();

            if (lesson == null)
                return NotFound(BaseResponse<LessonViewModel>.Fail("Không tìm thấy bài học"));

            var lessonVm = new LessonViewModel
            {
                Id = lesson.Id,
                LessonName = lesson.LessonName,
                Requirements = lesson.Requirements,
                ChapterID = lesson.ChapterID,
                GradeName = lesson.Chapter?.Grade?.GradeName ?? string.Empty,
                PresentationCount = lesson.Presentations.Count,
                FirstPresentationId = lesson.Presentations.Select(p => p.Id).FirstOrDefault()
            };

            return Ok(BaseResponse<LessonViewModel>.Ok(lessonVm, "Lấy chi tiết thành công"));
        }

        // POST: api/lessons
        [HttpPost]
        public async Task<ActionResult<BaseResponse<LessonDto>>> Create(LessonDto dto)
        {
            // Validate logic như trước
            var gradeExisted = await _graderRepository.GetByIdAsync(dto.GradeID);
            if (gradeExisted == null)
                return BadRequest(BaseResponse<LessonDto>.Fail("Khối lớp không tồn tại"));

            var chaperExisted = await _chapterRepository.GetByIdAsync(dto.ChapterID);
            if (chaperExisted == null || chaperExisted.GradeID != dto.GradeID)
                return BadRequest(BaseResponse<LessonDto>.Fail("Chương không tồn tại hoặc không thuộc khối lớp đã chọn"));

            var lesson = dto.Adapt<Lesson>();
            var created = await _repository.AddAsync(lesson);

            var result = created.Adapt<LessonDto>();

            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                BaseResponse<LessonDto>.Ok(result, "Tạo mới thành công"));
        }


        // PUT: api/lessons/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BaseResponse<object>>> Update(Guid id, Lesson lesson)
        {
            if (id != lesson.Id)
                return BadRequest(BaseResponse<object>.Fail("ID không khớp"));

            await _repository.UpdateAsync(lesson);
            return Ok(BaseResponse<object>.Ok(null, "Cập nhật thành công"));
        }

        // DELETE: api/lessons/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<BaseResponse<object>>> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return Ok(BaseResponse<object>.Ok(null, "Xóa thành công"));
        }

        [HttpGet("by-chapter/{chapterId:guid}")]
        public async Task<ActionResult<BaseResponse<IEnumerable<LessonViewModel>>>> GetByChapterId(Guid chapterId)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var userId))
            {
                return Unauthorized(BaseResponse<PresentationViewModel>.Fail("User is not authenticated."));
            }
            var lessons = await _repository.Query(l => l.ChapterID == chapterId && !l.IsDeleted)
                                           .Include(l => l.Presentations)
                                           .Select(lesson => new LessonViewModel
                                           {
                                               Id = lesson.Id,
                                               LessonName = lesson.LessonName,
                                               Requirements = lesson.Requirements,
                                               ChapterID = lesson.ChapterID,
                                               PresentationCount = lesson.Presentations.Count(p => !p.IsDeleted && p.UserID == userId),
                                               FirstPresentationId = lesson.Presentations.Where(p => !p.IsDeleted && p.UserID == userId).Select(p => p.Id).FirstOrDefault()
                                           })
                                           .ToListAsync();
            return Ok(BaseResponse<IEnumerable<LessonViewModel>>.Ok(lessons, "Lấy danh sách bài học theo chương thành công"));
        }
    }
}

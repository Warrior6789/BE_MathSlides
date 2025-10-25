using Mapster;
using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathSlidesBe.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChapterController : ControllerBase
    {
        private readonly IRepository<Chapter> _Chapterrepository;
        private readonly IRepository<Grade> _Graderepository;
        public ChapterController(IRepository<Chapter> chapterrepository, IRepository<Grade> gradeRepository)
        {
            _Chapterrepository = chapterrepository;
            _Graderepository = gradeRepository;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<Chapter>>>> GetAll()
        {
            var chapter = await _Chapterrepository.GetAllAsync();
            return Ok(BaseResponse<IEnumerable<Chapter>>.Ok(chapter,"Lấy danh sách thành công"));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BaseResponse<Chapter>>> GetById(Guid id)
        {
            var chapter = await _Chapterrepository.GetByIdAsync(id);
            if(chapter == null)
            {
                return NotFound(BaseResponse<Chapter>.Fail("Chương không tồn tại"));
            }
            return Ok(BaseResponse<Chapter>.Ok(chapter,"Lấy chương thành công"));
        }

        [HttpGet("paged")]
        public async Task<ActionResult<BaseResponse<PagedResult<Chapter>>>> GetPaged(
            int pageNumber = 1,
            int pageSize = 10,
            string? keyWorld = null,
            Guid? gradeId = null)
        {
            var query = _Chapterrepository.Query(c => !c.IsDeleted);
            if (!string.IsNullOrEmpty(keyWorld))
            {
                string lowerKeyWorld = keyWorld.ToLower();
                query = query.Where(c => c.ChapterName.ToLower().Contains(lowerKeyWorld));
            }
            if (gradeId.HasValue)
            {
                query = query.Where(c => c.GradeID == gradeId.Value);
            }

            var totalItem = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var pagedResult = new PagedResult<Chapter>()
            {
                Items = items,
                TotalItems = totalItem,
                PageIndex = pageNumber,
                PageSize = pageSize
            };
            return Ok(BaseResponse<PagedResult<Chapter>>.Ok(pagedResult,"Lấy danh sách chương phân trang thành công"));
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<Chapter>>> Create([FromBody] ChapterDto dto)
        {
            var entity = dto.Adapt<Chapter>();
            var chapter = await _Chapterrepository.AddAsync(entity);
            return Ok(BaseResponse<Chapter>.Ok(chapter,"Tạo chương thành công"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BaseResponse<ChapterDto>>> Update(Guid id, ChapterDto dto)
        {
           var chapter = await _Chapterrepository.GetByIdAsync(id);
              if(chapter == null)
              {
                 return NotFound(BaseResponse<Object>.Fail("Chương không tồn tại"));
              }
           var grade = await _Graderepository.GetByIdAsync(dto.GradeID);
              if(grade == null)
              {
                 return NotFound(BaseResponse<Chapter>.Fail("Khối không tồn tại"));
              }
            chapter.ChapterName = dto.ChapterName;
            chapter.GradeID = dto.GradeID;
            chapter.UpdatedAt = DateTime.UtcNow;

            await _Chapterrepository.UpdateAsync(chapter);
            return Ok(BaseResponse<Object>.Ok(null, "Cập nhật chương thành công"));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<BaseResponse<object>>> Delete(Guid id)
        {
            await _Chapterrepository.DeleteAsync(id);
            return Ok(BaseResponse<object>.Ok(null,"Xóa chương thành công"));
        }
    }
}

using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace MathSlidesBe.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolsController : ControllerBase
    {
        private readonly IRepository<School> _repository;
        public SchoolsController(IRepository<School> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<School>>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(BaseResponse<IEnumerable<School>>.Ok(result, "Lấy danh sách thành công"));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BaseResponse<School>>> GetById(Guid id)
        {
            var school = await _repository.GetByIdAsync(id);
            if (school == null)
            {
                return NotFound(BaseResponse<School>.Fail("Không tìm thấy trường"));
            }
            return Ok(BaseResponse<School>.Ok(school, "Lấy chi tiết thành công"));
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<School>>> Create([FromBody] SchoolDto dto)
        {
            var entity = dto.Adapt<School>();
            await _repository.AddAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, BaseResponse<School>.Ok(entity, "Tạo trường thành công"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BaseResponse<object>>> Update(Guid id, School school)
        {
            if (id != school.Id)
                return BadRequest(BaseResponse<object>.Fail("ID không khớp"));

            await _repository.UpdateAsync(school);
            return Ok(BaseResponse<object>.Ok(null, "Cập nhật thành công"));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<BaseResponse<School>>> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return Ok(BaseResponse<Object>.Ok(null, "Xoá trường thành công"));

        }

        [HttpGet("paged")]
        public async Task<ActionResult<BaseResponse<PagedResult<School>>>> GetPaged(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyworld = null)
        {
            var query = _repository.Query(x => !x.IsDeleted);
            if (!string.IsNullOrEmpty(keyworld))
            {
                var keywordLower = keyworld.ToLower();
                query.Where(x => x.SchoolName.ToLower().Contains(keywordLower) ||(x.Address != null && x.Address.ToLower().Contains(keywordLower)) || x.SchoolCode.ToLower().Contains(keywordLower));
            }
            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.SchoolName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();
            var pagedResult = new PagedResult<School>
            {
                Items = items,
                TotalItems = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(BaseResponse<PagedResult<School>>.Ok(pagedResult,"Lấy thành công"));
        }
    }
}

using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mapster;

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
            return Ok(BaseResponse<IEnumerable<School>>.Ok(result,"Lấy danh sách thành công"));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BaseResponse<School>>> GetById(Guid id)
        {
            var school = await _repository.GetByIdAsync(id);
            if(school == null)
            {
                return NotFound(BaseResponse<School>.Fail("Không tìm thấy trường"));
            }
            return Ok(BaseResponse<School>.Ok(school,"Lấy chi tiết thành công"));
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<School>>> Create([FromBody] SchoolDto dto)
        {
            var entity = dto.Adapt<School>();
            await _repository.AddAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, BaseResponse<School>.Ok(entity,"Tạo trường thành công"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BaseResponse<School>>> Update(Guid id, [FromBody] School school)
        {
            var existingSchool = await _repository.GetByIdAsync(id);
            if(existingSchool == null)
            {
                return NotFound(BaseResponse<School>.Fail("Không tìm thấy trường"));
            }
            await _repository.UpdateAsync(school);
            return Ok(BaseResponse<School>.Ok(school,"Cập nhật trường thành công"));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<BaseResponse<School>>> Delete(Guid id)
        {
             await _repository.DeleteAsync(id);
            return Ok(BaseResponse<Object>.Ok(null,"Xoá trường thành công"));

        }
}

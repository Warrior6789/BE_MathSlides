using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathSlidesBe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradesController : ControllerBase
    {
        private readonly IRepository<Grade> _repository;
        private readonly IWebHostEnvironment _environment;
        public GradesController(IRepository<Grade> repository, IWebHostEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }
        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<Grade>>>> GetAll()
        {
            var grades = await _repository.GetAllAsync();
            return Ok(BaseResponse<IEnumerable<Grade>>.Ok(grades, "Lấy danh sách khối lớp thành công"));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BaseResponse<Grade>>> GetById(Guid id)
        {
            var grade = await _repository.GetByIdAsync(id);
            if (grade == null)
                return NotFound(BaseResponse<Grade>.Fail("Không tìm thấy khối lớp"));

            return Ok(BaseResponse<Grade>.Ok(grade, "Lấy chi tiết thành công"));
        }

        [HttpGet("paged")]
        public async Task<ActionResult<BaseResponse<PagedResult<Grade>>>> GetPaged(
       int pageIndex = 1,
       int pageSize = 10,
       string? keyword = null,
       bool sortDesc = false)
        {
            var query = _repository.Query(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(g => g.GradeName.ToLower().Contains(lowerKeyword));
            }

            query = sortDesc
                ? query.OrderByDescending(g => g.DisplayOrder)
                : query.OrderBy(g => g.DisplayOrder);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedResult = new PagedResult<Grade>
            {
                Items = items,
                TotalItems = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Ok(BaseResponse<PagedResult<Grade>>.Ok(pagedResult, "Lấy dữ liệu phân trang thành công"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] GradeDto dto)
        {
            string fileName = null;
            string fileUrl = null;

            if (dto.BackgroundImage != null && dto.BackgroundImage.Length > 0)
            {
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileNameOrigin = dto.BackgroundImage.FileName;  
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.BackgroundImage.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.BackgroundImage.CopyToAsync(stream);
                }

                fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            }

            var grade = new Grade
            {
                GradeName = dto.GradeName,
                DisplayOrder = dto.DisplayOrder,
                BackgroundFileName = fileName,
                BackgroundUrl = fileUrl
            };

            var created = await _repository.AddAsync(grade);
            return Ok(BaseResponse<Grade>.Ok(created, "Tạo khối lớp thành công"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BaseResponse<object>>> Update(Guid id, [FromForm] GradeDto dto)
        {
            var existingGrade = await _repository.GetByIdAsync(id);
            if (existingGrade == null)
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy khối lớp"));

            string fileName = existingGrade.BackgroundFileName;
            string fileUrl = existingGrade.BackgroundUrl;

            if (dto.BackgroundImage != null && dto.BackgroundImage.Length > 0)
            {
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                if (!string.IsNullOrEmpty(existingGrade.BackgroundFileName))
                {
                    var oldFilePath = Path.Combine(uploadsPath, existingGrade.BackgroundFileName);
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.BackgroundImage.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.BackgroundImage.CopyToAsync(stream);
                }

                fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            }

            existingGrade.GradeName = dto.GradeName;
            existingGrade.DisplayOrder = dto.DisplayOrder;
            existingGrade.BackgroundFileName = fileName;
            existingGrade.BackgroundUrl = fileUrl;

            await _repository.UpdateAsync(existingGrade);

            return Ok(BaseResponse<object>.Ok(null, "Cập nhật khối lớp thành công"));
        }


        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<BaseResponse<object>>> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return Ok(BaseResponse<object>.Ok(null, "Xóa khối lớp thành công"));
        }
    }
}

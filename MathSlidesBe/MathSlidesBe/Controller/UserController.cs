using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Entity.Enum;
using MathSlidesBe.Models.Dto;
using MathSlidesBe.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathSlidesBe.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MathSlidesDbContext _context;
        private readonly IRepository<User> _repository;
        public UserController(MathSlidesDbContext context, IRepository<User> repository)
        {
            _context = context;
            _repository = repository;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var checkSchool = await _context.Schools.FirstOrDefaultAsync(s => s.Id == dto.SchoolId);
            if(checkSchool == null)
            {
               return BadRequest(new { message = "Trường không hợp lệ." });
            }
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if(existingUser != null)
            {
                return BadRequest(new { message = "Email đã được sử dụng." });
            }
            var passwordHash = Helper.HashPassword(dto.Password);
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.phoneNumber,
                PasswordHash = passwordHash,
                Role = dto.Role,
                SchoolID = dto.SchoolId,
                UserStatus = UserStatus.Pending
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.SchoolID,
            });
        }
        [Authorize]
        [HttpGet("MyProfile")]
        public async Task<ActionResult<BaseResponse<UserProfile>>> MyProfile()
        {
            var currentUser = User.Identity?.Name;
            var userExisted = _context.Users.Include(it => it.School).FirstOrDefault(u => u.Email == currentUser) ?? throw new ArgumentNullException(nameof(currentUser), "Người dùng không tồn tại");
            var dataUser = new UserProfile
            {
                FullName = userExisted.FullName,
                Email = userExisted.Email,
                PhoneNumber = userExisted.PhoneNumber,
                Role = userExisted.Role.ToString(),
                School = userExisted.School?.SchoolName,
                Status = userExisted.UserStatus.ToString()
            };
            return Ok(BaseResponse<UserProfile>.Ok(dataUser,"Lấy chi tiết thành công"));
        }
    }
}

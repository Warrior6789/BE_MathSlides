﻿using MathSlidesBe.BaseRepo;
using MathSlidesBe.Common;
using MathSlidesBe.Entity;
using MathSlidesBe.Entity.Enum;
using MathSlidesBe.Models.Dto;
using MathSlidesBe.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<BaseResponse<PagedResult<User>>>> GetPaged(int pageIndex = 1, int pageSize = 10, string? search = null, UserStatus? status = null)
        {
            Expression<Func<User, bool>> filter = u => true;
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                filter = (u => u.FullName.ToLower().Contains(search) || u.Email.ToLower().Contains(search) || u.PhoneNumber.ToLower().Contains(search));
            }

            var query = _repository.Query(filter);

            if (status.HasValue)
            {
                query = query.Where(u => u.UserStatus == status.Value);
            }

            var totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(u => u.UpdatedAt).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            foreach (var user in items)
            {
                user.PasswordHash = null;
            }

            var result = new PagedResult<User>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems
            };
            return Ok(BaseResponse<PagedResult<User>>.Ok(result,"Lấy dữ liệu phân trang thành công "));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/update-status")]
        public async Task<ActionResult<BaseResponse<Object>>> UpdateUserStatus(Guid id, [FromQuery] UserStatus status)
        {
            var userExisted = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if(userExisted == null)
            {
                return NotFound(BaseResponse<Object>.Fail("Không tìm thấy người dùng"));
            }
            userExisted.UserStatus = status;
            userExisted.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(userExisted);
            await _context.SaveChangesAsync();
            return Ok(BaseResponse<Object>.Ok(null,"Cập nhật trạng thái người dùng thành công")); 

        }
    }
}

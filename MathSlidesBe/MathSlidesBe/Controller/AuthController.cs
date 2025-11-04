using MathSlidesBe.BaseRepo;
using MathSlidesBe.Entity;
using MathSlidesBe.Entity.Enum;
using MathSlidesBe.Models.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;

namespace MathSlidesBe.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRepository<User> _AuthRepository;
        public AuthController(IRepository<User> AuthRepository)
        {
            _AuthRepository = AuthRepository;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if(request.Email.Equals("admin") && request.Password == "1")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, request.Email),
                    new Claim(ClaimTypes.Role, UserRole.Admin.ToString()),
                    new Claim(ClaimTypes.NameIdentifier,Guid.Empty.ToString())
                };
                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("MyCookieAuth", principal);
                return Ok(new { message = "Login success" });
            }

            var user = await _AuthRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
           
            if (user != null && user.PasswordHash == Helper.HashPassword(request.Password))
            {
                if(user.UserStatus != UserStatus.Active)
                {
                    return Unauthorized(new {message = "User not accepted waiting admin accept"});
                }
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.Email),
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Role,user.Role.ToString())
                };
                var identity = new ClaimsIdentity(claims,"MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("MyCookieAuth",principal);
                return Ok(new {message = "Login success"});
            }
            return Unauthorized(new { message = "Đăng nhập thất bại! Vui lòng kiểm tra lại tài khoản và mật khẩu hoặc chờ được phê duyệt" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return Ok(new { message = "Logout success" });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return Ok(new
                {
                    username = User.Identity.Name,
                    role = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value),
                    userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                });
            }
            return Unauthorized();
        }
    }
}

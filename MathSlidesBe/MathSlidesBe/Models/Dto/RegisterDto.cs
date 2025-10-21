using MathSlidesBe.Entity.Enum;
using System.ComponentModel.DataAnnotations;

namespace MathSlidesBe.Models.Dto
{
    public class RegisterDto
    {
        [Required, MaxLength(255)]
        public string FullName { get; set; }
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }
        [MaxLength(20)]
        public string phoneNumber { get; set; }
        [Required, MinLength(6)]
        public string Password { get; set; }
        [Required]
        public Guid SchoolId { get; set; }
        public UserRole Role { get; set; } = UserRole.Teacher;
    }
}

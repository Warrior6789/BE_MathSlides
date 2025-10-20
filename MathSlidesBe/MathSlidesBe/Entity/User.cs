using MathSlidesBe.Entity.Enum;
using System.ComponentModel.DataAnnotations;

namespace MathSlidesBe.Entity
{
    public class User : BaseEntity
    {
        [Required, MaxLength(255)]
        public string FullName { get; set; }

        [Required, MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; }

        // FK
        public Guid? SchoolID { get; set; }
        public School? School { get; set; }

        // Navigation
        public ICollection<Presentation> Presentations { get; set; } = new List<Presentation>();
        public UserStatus UserStatus { get; set; }
    }
}

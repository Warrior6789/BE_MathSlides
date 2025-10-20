using System.ComponentModel.DataAnnotations;

namespace MathSlidesBe.Entity
{
    public class School : BaseEntity
    {

        [Required, MaxLength(255)]
        public string SchoolName { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public string SchoolCode { get; set; }

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}

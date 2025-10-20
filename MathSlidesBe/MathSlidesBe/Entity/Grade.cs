using System.ComponentModel.DataAnnotations;

namespace MathSlidesBe.Entity
{
    public class Grade : BaseEntity
    {
        [Required, MaxLength(100)]
        public string GradeName { get; set; }
        [MaxLength(255)]
        public string BackgroundFileName { get; set; }

        [MaxLength(500)]
        public string BackgroundUrl { get; set; }

        public int DisplayOrder { get; set; }

        // Navigation
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}

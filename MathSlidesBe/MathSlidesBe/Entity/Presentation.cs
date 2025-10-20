using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace MathSlidesBe.Entity
{
    public class Presentation : BaseEntity
    {
        [Required, MaxLength(255)]
        public string Title { get; set; }

        // FK
        public Guid UserID { get; set; }
        public User User { get; set; }

        public Guid LessonID { get; set; }
        public Lesson Lesson { get; set; }

        // Navigation
        public ICollection<Slide> Slides { get; set; } = new List<Slide>();
    }
}

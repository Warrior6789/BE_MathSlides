using System.ComponentModel.DataAnnotations;

namespace MathSlidesBe.Entity
{
    public class Lesson : BaseEntity
    {
        [Required, MaxLength(255)]
        public string LessonName { get; set; }

        public string? Requirements { get; set; }

        // FK
        public Guid ChapterID { get; set; }
        public Chapter? Chapter { get; set; }

        // Navigation
        public ICollection<Presentation> Presentations { get; set; } = new List<Presentation>();

        internal T Adapt<T>()
        {
            throw new NotImplementedException();
        }
    }
}

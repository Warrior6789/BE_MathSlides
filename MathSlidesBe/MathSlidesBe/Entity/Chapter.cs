using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace MathSlidesBe.Entity
{
    public class Chapter : BaseEntity
    {
        [Required, MaxLength(255)]
        public string ChapterName { get; set; }

        // FK
        public Guid GradeID { get; set; }
        public Grade? Grade { get; set; }

        // Navigation
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}

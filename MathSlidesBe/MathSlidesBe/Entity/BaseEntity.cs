using System.ComponentModel.DataAnnotations;

namespace MathSlidesBe.Entity
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace MathSlidesBe.Entity
{
    public class Component : BaseEntity
    {
        public Guid SlideID { get; set; }
        public Slide Slide { get; set; }

        [Required, MaxLength(100)]
        public string ComponentType { get; set; }

        // JSON string
        public string Properties { get; set; }

        public int ZIndex { get; set; }
    }
}

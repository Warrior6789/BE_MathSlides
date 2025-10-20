namespace MathSlidesBe.Entity
{
    public class Slide : BaseEntity
    {
        public Guid PresentationID { get; set; }
        public Presentation Presentation { get; set; }

        public int PageNumber { get; set; }

        // Navigation
        public ICollection<Component> Components { get; set; } = new List<Component>();
    }
}


namespace MathSlidesBe.Models.ViewModel
{
    public class PresentationViewModel
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public Guid UserID { get; set; }
        public Guid LessonID { get; set; }
        public List<SlideViewModel>? Slides { get; set; }
    }
}

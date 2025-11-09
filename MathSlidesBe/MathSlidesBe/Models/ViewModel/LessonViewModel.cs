namespace MathSlidesBe.Models.ViewModel
{
    public class LessonViewModel
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string? Requirements { get; set; }
        public Guid ChapterID { get; set; }
        public string GradeName { get; set; }
        public int PresentationCount { get; set; }
        public Guid? FirstPresentationId { get; set; }
    }
}

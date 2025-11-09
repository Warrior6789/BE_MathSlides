namespace MathSlidesBe.Models.Dto
{
    public record LessonDto
    {
        public string LessonName { get; set; }

        public string? Requirements { get; set; }

        public Guid ChapterID { get; set; }
        public Guid GradeID { get; set; }
    }
}

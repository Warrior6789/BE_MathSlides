namespace MathSlidesBe.Models.Dto
{
    public record SchoolDto

    
    {
        public string SchoolName { get; set; }
        public string? Address { get; set; }
        public string SchoolCode { get; set; }
    }
    
    public record GradeDto
    {
        public string GradeName { get; set; }

        public int DisplayOrder { get; set; }
        public IFormFile? BackgroundImage { get; set; }
    }
    public record ChapterDto
    {
        public string ChapterName { get; set; }
        public Guid GradeID { get; set; }
    }
    public record LessonDto
    {
        public string LessonName { get; set; }

        public string? Requirements { get; set; }

        public Guid ChapterID { get; set; }
        public Guid GradeID { get; set; }

        internal T Adapt<T>()
        {
            throw new NotImplementedException();
        }
    }
}


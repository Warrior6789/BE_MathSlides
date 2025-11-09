namespace MathSlidesBe.Models.Dto
{
    public record GradeDto
    {
        public string GradeName { get; set; }

        public int DisplayOrder { get; set; }
        public IFormFile? BackgroundImage { get; set; }
    }
}

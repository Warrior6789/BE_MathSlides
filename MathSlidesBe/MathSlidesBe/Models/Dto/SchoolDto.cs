namespace MathSlidesBe.Models.Dto
{
    public record SchoolDto
    {
        public string SchoolName { get; set; }
        public string? Address { get; set; }
        public string SchoolCode { get; set; }
    }
}


namespace MathSlidesBe.Models.ViewModel
{
    public class UserProfile
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public string? School { get; set; }
        public List<Guid>? Slides { get; set; }
        public string Status { get; set; }
    }
}

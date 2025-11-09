namespace MathSlidesBe.Models.ViewModel
{
    public class SlideViewModel
    {
        public Guid? Id { get; set; }
        public Guid? PresentationID { get; set; }
        public int PageNumber { get; set; }
        public List<ComponentViewModel>? Components { get; set; }
    }

}

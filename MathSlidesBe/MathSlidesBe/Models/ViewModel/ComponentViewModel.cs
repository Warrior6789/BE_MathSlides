using MathSlidesBe.Controllers;

namespace MathSlidesBe.Models.ViewModel
{
    public class ComponentViewModel
    {
        public Guid? Id { get; set; }
        public Guid? SlideID { get; set; }
        public string ComponentType { get; set; }
        public ComponentPropertiesViewModel? Properties { get; set; }
        public int ZIndex { get; set; }
    }
}

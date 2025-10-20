using System.ComponentModel;

namespace MathSlidesBe.Entity.Enum
{
    public enum UserRole
    {
        [Description("Staff - Ministry of Education")]
        Admin = 1,
        [Description("Teacher")]
        Teacher = 2
    }
}

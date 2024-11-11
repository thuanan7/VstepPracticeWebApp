using Microsoft.AspNetCore.Identity;
using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.Entities;

public class User : IdentityUser<int>
{
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Exam> CreatedExams { get; set; } = default!;
    public virtual ICollection<StudentAttempt> StudentAttempts { get; set; } = default!;
}

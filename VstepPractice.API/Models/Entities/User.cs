using Microsoft.AspNetCore.Identity;

namespace VstepPractice.API.Models.Entities;

public class User : IdentityUser<int>, IEntity<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Exam> CreatedExams { get; set; } = default!;
    public virtual ICollection<StudentAttempt> StudentAttempts { get; set; } = default!;
}

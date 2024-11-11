using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.Entities;

public class StudentAttempt : BaseEntity
{
    public int UserId { get; set; }
    public int ExamId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

    // Navigation properties
    public virtual User User { get; set; } = default!;
    public virtual Exam Exam { get; set; } = default!;
    public virtual ICollection<Answer> Answers { get; set; } = default!;
}

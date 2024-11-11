using System.ComponentModel.DataAnnotations.Schema;

namespace VstepPractice.API.Models.Entities;

public class Exam : BaseEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int CreatedById { get; set; }

    // Navigation properties
    [ForeignKey("CreatedById")]
    public virtual User CreatedBy { get; set; } = default!;
    public virtual ICollection<Section> Sections { get; set; } = default!;
    public virtual ICollection<StudentAttempt> StudentAttempts { get; set; } = default!;
}

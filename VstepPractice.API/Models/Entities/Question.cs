using System.ComponentModel.DataAnnotations.Schema;
using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.Entities;

public class Question : BaseEntity
{
    public int SectionId { get; set; }
    public string? QuestionText { get; set; }
    public string? MediaUrl { get; set; }  // for listening section
    public int Points { get; set; } = 1;

    // Navigation properties
    [ForeignKey("SectionId")]
    public virtual Section Section { get; set; } = default!;
    public virtual ICollection<QuestionOption> Options { get; set; } = default!;
}

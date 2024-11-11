using System.ComponentModel.DataAnnotations.Schema;

namespace VstepPractice.API.Models.Entities;

public class QuestionOption : BaseEntity
{
    public int QuestionId { get; set; }
    public string? OptionText { get; set; }
    public bool IsCorrect { get; set; } = false;

    // Navigation property
    [ForeignKey("QuestionId")]
    public virtual Question Question { get; set; } = default!;
}

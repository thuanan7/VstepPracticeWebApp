using System.ComponentModel.DataAnnotations.Schema;
using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.Entities;

public class Question : BaseEntity
{
    public int SectionId { get; set; }
    public int PartId { get; set; }
    public int PassageId { get; set; }
    public string? QuestionText { get; set; }
    public string? MediaUrl { get; set; }
    public int Points { get; set; } = 1;
    public int OrderNum { get; set; }

    // Navigation properties
    [ForeignKey("SectionId")]
    public virtual Section Section { get; set; } = default!;
    [ForeignKey("PartId")]
    public virtual SectionPart Part { get; set; } = default!;
    [ForeignKey("PassageId")]
    public virtual Passage Passage { get; set; } = default!;
    public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}

using System.ComponentModel.DataAnnotations.Schema;

namespace VstepPractice.API.Models.Entities;

public class Passage : BaseEntity
{
    public int SectionId { get; set; }
    public int PartId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? AudioUrl { get; set; }
    public int OrderNum { get; set; }

    // Navigation properties
    [ForeignKey("SectionId")]
    public virtual Section Section { get; set; } = default!;
    [ForeignKey("PartId")]
    public virtual SectionPart Part { get; set; } = default!;
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}

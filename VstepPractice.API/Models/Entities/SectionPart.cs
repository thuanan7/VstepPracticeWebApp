using System.ComponentModel.DataAnnotations.Schema;

namespace VstepPractice.API.Models.Entities;

public class SectionPart : BaseEntity
{
    public int SectionId { get; set; }
    public int PartNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public int OrderNum { get; set; }

    // Navigation properties
    [ForeignKey("SectionId")]
    public virtual Section Section { get; set; } = default!;
    public virtual ICollection<Passage> Passages { get; set; } = new List<Passage>();
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}

using System.ComponentModel.DataAnnotations.Schema;

namespace VstepPractice.API.Models.Entities;

public class WritingAssessment : BaseEntity
{
    public int AnswerId { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal TaskAchievement { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal CoherenceCohesion { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal LexicalResource { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal GrammarAccuracy { get; set; }

    public string DetailedFeedback { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public decimal TotalScore => Math.Round((TaskAchievement + CoherenceCohesion + LexicalResource + GrammarAccuracy), 1);

    [ForeignKey(nameof(AnswerId))]
    public virtual Answer Answer { get; set; } = default!;
}

﻿using System.ComponentModel.DataAnnotations.Schema;
using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.Entities;

public class Section : BaseEntity
{
    public int ExamId { get; set; }
    public SectionType Type { get; set; }
    public string? Title { get; set; }
    public string? Instructions { get; set; }
    public int OrderNum { get; set; }

    // Navigation properties
    [ForeignKey("ExamId")]
    public virtual Exam Exam { get; set; } = default!;
    public virtual ICollection<SectionPart> Parts { get; set; } = new List<SectionPart>();
    public virtual ICollection<Passage> Passages { get; set; } = new List<Passage>();
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
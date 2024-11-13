using System.ComponentModel.DataAnnotations;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Models.DTOs.Questions.Requests;

namespace VstepPractice.API.Models.DTOs.Sections.Requests;

public class CreateSectionRequest
{
    [Required]
    public SectionType Type { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Instructions { get; set; }

    [Required]
    [Range(1, 10)]
    public int OrderNum { get; set; }

    [Required]
    public List<CreateQuestionRequest> Questions { get; set; } = new();
}

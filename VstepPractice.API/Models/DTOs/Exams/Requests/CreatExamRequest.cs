using System.ComponentModel.DataAnnotations;
using VstepPractice.API.Models.DTOs.Sections.Requests;

namespace VstepPractice.API.Models.DTOs.Exams.Requests;

public class CreateExamRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public List<CreateSectionRequest> Sections { get; set; } = new();
}
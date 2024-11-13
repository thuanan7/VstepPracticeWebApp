using System.ComponentModel.DataAnnotations;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Models.DTOs.SectionParts.Requests;

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
    public int OrderNum { get; set; }

    [Required]
    public List<CreateSectionPartRequest> Parts { get; set; } = new();
}
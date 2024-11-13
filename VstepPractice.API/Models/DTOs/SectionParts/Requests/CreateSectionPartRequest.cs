using System.ComponentModel.DataAnnotations;
using VstepPractice.API.Models.DTOs.Passage.Requests;

namespace VstepPractice.API.Models.DTOs.SectionParts.Requests;

public class CreateSectionPartRequest
{
    [Required]
    public int PartNumber { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Instructions { get; set; }

    [Required]
    public int OrderNum { get; set; }

    [Required]
    public List<CreatePassageRequest> Passages { get; set; } = new();
}

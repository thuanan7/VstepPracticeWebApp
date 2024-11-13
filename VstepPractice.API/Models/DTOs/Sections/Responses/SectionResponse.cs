using VstepPractice.API.Common.Enums;
using VstepPractice.API.Models.DTOs.SectionParts.Responses;

namespace VstepPractice.API.Models.DTOs.Sections.Responses;

public class SectionResponse
{
    public int Id { get; set; }
    public SectionType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public int OrderNum { get; set; }
    public List<SectionPartResponse> Parts { get; set; } = new();
}
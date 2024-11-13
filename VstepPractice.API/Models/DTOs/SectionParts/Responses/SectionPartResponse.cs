using VstepPractice.API.Models.DTOs.Passage.Responses;

namespace VstepPractice.API.Models.DTOs.SectionParts.Responses;

public class SectionPartResponse
{
    public int Id { get; set; }
    public int PartNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public int OrderNum { get; set; }
    public List<PassageResponse> Passages { get; set; } = new();
}
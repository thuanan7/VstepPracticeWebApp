using VstepPractice.API.Models.DTOs.Sections.Responses;
using VstepPractice.API.Models.DTOs.Users.Responses;

namespace VstepPractice.API.Models.DTOs.Exams.Responses;

public class ExamResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserDto CreatedBy { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public List<SectionResponse> Sections { get; set; } = new();
}

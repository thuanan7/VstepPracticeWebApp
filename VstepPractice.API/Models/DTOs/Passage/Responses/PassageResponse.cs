using VstepPractice.API.Models.DTOs.Questions.Responses;

namespace VstepPractice.API.Models.DTOs.Passage.Responses;

public class PassageResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? AudioUrl { get; set; }
    public int OrderNum { get; set; }
    public List<QuestionResponse> Questions { get; set; } = new();
}
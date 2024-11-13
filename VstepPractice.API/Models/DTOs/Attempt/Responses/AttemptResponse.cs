using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.DTOs.Attempt.Responses;

public class AttemptResponse
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public AttemptStatus Status { get; set; }
    public List<AnswerResponse> Answers { get; set; } = new();
}
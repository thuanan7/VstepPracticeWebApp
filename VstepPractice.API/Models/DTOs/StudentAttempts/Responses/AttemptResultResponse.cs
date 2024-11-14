namespace VstepPractice.API.Models.DTOs.StudentAttempts.Responses;

public class AttemptResultResponse
{
    public int Id { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Dictionary<string, decimal> SectionScores { get; set; } = new();
    public WritingSectionScore? WritingDetails { get; set; }
    public decimal FinalScore { get; set; }
    public List<AnswerResponse> Answers { get; set; } = new();
}
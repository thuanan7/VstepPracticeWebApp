namespace VstepPractice.API.Models.DTOs.AI;

public class WritingAssessmentResponse
{
    public decimal TaskAchievement { get; set; }
    public decimal CoherenceCohesion { get; set; }
    public decimal LexicalResource { get; set; }
    public decimal GrammarAccuracy { get; set; }
    public string DetailedFeedback { get; set; } = string.Empty;

    public decimal TotalScore => (TaskAchievement + CoherenceCohesion + LexicalResource + GrammarAccuracy) / 4;
}

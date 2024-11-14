namespace VstepPractice.API.Models.DTOs.StudentAttempts.Responses;

public class WritingTaskScore
{
    public int TaskNumber { get; set; }
    public decimal TaskAchievement { get; set; }
    public decimal CoherenceCohesion { get; set; }
    public decimal LexicalResource { get; set; }
    public decimal GrammarAccuracy { get; set; }
    public decimal TotalScore => TaskAchievement + CoherenceCohesion + LexicalResource + GrammarAccuracy;
}

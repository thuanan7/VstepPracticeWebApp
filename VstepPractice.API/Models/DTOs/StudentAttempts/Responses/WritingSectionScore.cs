using VstepPractice.API.Common.Utils;

namespace VstepPractice.API.Models.DTOs.StudentAttempts.Responses;

public class WritingSectionScore
{
    public List<WritingTaskScore> TaskScores { get; set; } = new();
    public decimal FinalScore => VstepScoreCalculator.CalculateWritingScore(
        TaskScores[0].TotalScore,
        TaskScores[1].TotalScore);
}

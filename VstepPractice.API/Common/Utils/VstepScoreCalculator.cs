namespace VstepPractice.API.Common.Utils;

public static class VstepScoreCalculator
{
    public static decimal RoundToNearestHalf(decimal score)
    {
        return Math.Round(score * 2, MidpointRounding.AwayFromZero) / 2;
    }

    public static decimal CalculateListeningScore(int correctAnswers)
    {
        // Convert to scale of 10 and round to nearest 0.5
        decimal rawScore = (decimal)correctAnswers * 10 / 35;
        return RoundToNearestHalf(rawScore);
    }

    public static decimal CalculateReadingScore(IEnumerable<decimal> partScores)
    {
        // Reading is already on scale of 10
        return RoundToNearestHalf(partScores.Sum());
    }

    public static decimal CalculateWritingScore(decimal task1Score, decimal task2Score)
    {
        // (Task1 + (Task2 * 2)) / 3
        // Tasks scores are already on scale of 0-10 (sum of four 2.5-point criteria)
        decimal totalScore = (task1Score + (task2Score * 2)) / 3;
        return RoundToNearestHalf(totalScore);
    }

    public static decimal CalculateFinalScore(
        decimal listeningScore,
        decimal readingScore,
        decimal writingScore)
    {
        // (Listening + Reading + Writing) / 3
        decimal totalScore = (listeningScore + readingScore + writingScore) / 3;
        return RoundToNearestHalf(totalScore);
    }
}

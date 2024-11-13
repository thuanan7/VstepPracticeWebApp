namespace VstepPractice.API.Models.DTOs.StudentAttempts.Responses;

public class AnswerResponse
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int? SelectedOptionId { get; set; }
    public string? EssayAnswer { get; set; }
    public string? AiFeedback { get; set; }
    public decimal? Score { get; set; }
    public bool IsCorrect { get; set; }  // For multiple choice questions
}

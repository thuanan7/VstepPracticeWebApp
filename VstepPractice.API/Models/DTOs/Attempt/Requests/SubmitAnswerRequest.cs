namespace VstepPractice.API.Models.DTOs.Attempt.Requests;

public class SubmitAnswerRequest
{
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }  // For multiple choice
    public string? EssayAnswer { get; set; }    // For writing section
}

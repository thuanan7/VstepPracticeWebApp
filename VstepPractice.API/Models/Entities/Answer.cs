using System.ComponentModel.DataAnnotations.Schema;

namespace VstepPractice.API.Models.Entities;

public class Answer : BaseEntity
{
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }  // For multiple choice questions
    public string? EssayAnswer { get; set; }     // For writing questions
    public string? AiFeedback { get; set; }
    public decimal? Score { get; set; }         // Điểm số (cho cả trắc nghiệm và tự luận)

    // Navigation properties
    public virtual StudentAttempt Attempt { get; set; } = default!;
    public virtual Question Question { get; set; } = default!;
    public virtual QuestionOption? SelectedOption { get; set; }
}
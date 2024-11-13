using System.ComponentModel.DataAnnotations;

namespace VstepPractice.API.Models.DTOs.Questions.Requests;

public class CreateQuestionOptionRequest
{
    [Required]
    [StringLength(500)]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

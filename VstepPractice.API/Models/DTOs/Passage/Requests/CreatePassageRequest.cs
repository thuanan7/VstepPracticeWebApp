using System.ComponentModel.DataAnnotations;
using VstepPractice.API.Models.DTOs.Questions.Requests;

namespace VstepPractice.API.Models.DTOs.Passage.Requests;

public class CreatePassageRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? AudioUrl { get; set; }

    [Required]
    public int OrderNum { get; set; }

    [Required]
    public List<CreateQuestionRequest> Questions { get; set; } = new();
}
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.AI;

namespace VstepPractice.API.Services.AI;


public interface IAiScoringService
{
    Task<Result<WritingAssessmentResponse>> AssessEssayAsync(
        int answerId,
        string essay,
        string prompt,
        CancellationToken cancellationToken = default);
}

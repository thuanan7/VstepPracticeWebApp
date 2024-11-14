using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.AI;

namespace VstepPractice.API.Services.AI;


public interface IAiScoringService
{
    Task<Result<WritingAssessmentResponse>> AssessEssayAsync(
        EssayScoringTask task,
        CancellationToken cancellationToken = default);
}

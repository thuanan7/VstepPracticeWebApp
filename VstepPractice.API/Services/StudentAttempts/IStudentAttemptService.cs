using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Attempt.Requests;
using VstepPractice.API.Models.DTOs.Attempt.Responses;

namespace VstepPractice.API.Services.StudentAttempts;

public interface IStudentAttemptService
{
    Task<Result<AttemptResponse>> StartAttemptAsync(
        int userId,
        StartAttemptRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AnswerResponse>> SubmitAnswerAsync(
        int userId,
        int attemptId,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AttemptResultResponse>> FinishAttemptAsync(
        int userId,
        FinishAttemptRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AttemptResultResponse>> GetAttemptResultAsync(
        int userId,
        int attemptId,
        CancellationToken cancellationToken = default);
}
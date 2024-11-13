using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Exams.Requests;
using VstepPractice.API.Models.DTOs.Exams.Responses;
using VstepPractice.API.Models.DTOs.Questions.Requests;
using VstepPractice.API.Models.DTOs.Questions.Responses;
using VstepPractice.API.Models.DTOs.Sections.Requests;

namespace VstepPractice.API.Services.Exams;

public interface IExamService
{
    Task<Result<PagedResult<ExamResponse>>> GetExamsAsync(
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<Result<ExamResponse>> GetExamByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Result<ExamResponse>> CreateExamAsync(
        int userId,
        CreateExamRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ExamResponse>> UpdateExamAsync(
        int id,
        int userId,
        UpdateExamRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteExamAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ExamResponse>>> GetExamsByUserIdAsync(int userId, int pageIndex, int pageSize, CancellationToken cancellationToken);
   
}

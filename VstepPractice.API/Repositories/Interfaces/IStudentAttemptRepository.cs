using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Repositories.Interfaces;

public interface IStudentAttemptRepository : IRepositoryBase<StudentAttempt, int>
{
    Task<bool> HasInProgressAttempt(
        int userId,
        int examId,
        CancellationToken cancellationToken = default);

    Task<StudentAttempt?> GetAttemptWithDetailsAsync(
        int attemptId,
        CancellationToken cancellationToken = default);
}

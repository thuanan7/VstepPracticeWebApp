using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Repositories.Interfaces;

public interface IStudentAttemptRepository : IRepositoryBase<StudentAttempt, int>
{
    Task<StudentAttempt?> GetAttemptWithDetailsAsync(
        int attemptId,
        CancellationToken cancellationToken = default);
}

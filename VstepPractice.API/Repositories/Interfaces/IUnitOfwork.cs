namespace VstepPractice.API.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IExamRepository ExamRepository { get; }
    IUserRepository UserRepository { get; }
    IQuestionOptionRepository QuestionOptions { get; }
    IStudentAttemptRepository StudentAttemptRepository { get; }
    IAnswerRepository AnswerRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using VstepPractice.API.Data;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;

namespace VstepPractice.API.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    private IUserRepository? _userRepository;
    private IExamRepository? _examRepository;
    private IQuestionOptionRepository? _questionOptionRepository;
    private IStudentAttemptRepository? _studentAttemptRepository;
    private IAnswerRepository? _answerRepository;

    public UnitOfWork(
        ApplicationDbContext context,
        UserManager<User> userManager) // Inject các dependencies cần thiết
    {
        _context = context;
        _userManager = userManager;
        // Khởi tạo các repositories khi cần
        _examRepository = new ExamRepository(_context);
        _userRepository = new UserRepository(_context, userManager);
    }

    public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context,
        _userManager); // Inject UserManager
    public IExamRepository ExamRepository => _examRepository ??= new ExamRepository(_context);
    
    public IQuestionOptionRepository QuestionOptions => _questionOptionRepository ??= new QuestionOptionRepository(_context);

    public IStudentAttemptRepository StudentAttemptRepository =>
        _studentAttemptRepository ??= new StudentAttemptRepository(_context);

    public IAnswerRepository AnswerRepository =>
        _answerRepository ??= new AnswerRepository(_context);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
            _transaction?.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

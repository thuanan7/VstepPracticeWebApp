using Microsoft.EntityFrameworkCore;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Data;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;

namespace VstepPractice.API.Repositories.Implementations;

public class StudentAttemptRepository : RepositoryBase<StudentAttempt, int>, IStudentAttemptRepository
{
    public StudentAttemptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasInProgressAttempt(
        int userId,
        int examId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StudentAttempts
            .AnyAsync(a =>
                a.UserId == userId &&
                a.ExamId == examId &&
                a.Status == AttemptStatus.InProgress,
                cancellationToken);
    }

    public async Task<StudentAttempt?> GetAttemptWithDetailsAsync(
        int attemptId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StudentAttempts
            .Include(a => a.Exam)
                .ThenInclude(e => e.Sections)
                    .ThenInclude(s => s.Questions)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.Question)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOption)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);
    }
}

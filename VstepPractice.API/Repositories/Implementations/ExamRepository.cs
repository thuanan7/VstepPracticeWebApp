using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Data;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;

namespace VstepPractice.API.Repositories.Implementations;

public class ExamRepository : RepositoryBase<Exam, int>, IExamRepository
{
    public ExamRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Exam>> GetPagedAsync(
        Expression<Func<Exam, bool>>? predicate = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Exam>().AsQueryable();

        // Apply predicate if provided
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        // Include related data
        query = query
            .Include(e => e.CreatedBy)
            .Include(e => e.Sections.OrderBy(s => s.OrderNum))
                .ThenInclude(s => s.Questions)
                    .ThenInclude(q => q.Options);

        return await PagedResult<Exam>.CreateAsync(query, pageIndex, pageSize, cancellationToken);
    }

    public override async Task<Exam?> FindByIdAsync(
        int id,
        CancellationToken cancellationToken = default,
        params Expression<Func<Exam, object>>[] includeProperties)
    {
        var query = _context.Set<Exam>().AsQueryable();

        // Include default relations
        query = query
            .Include(e => e.CreatedBy)
            .Include(e => e.Sections.OrderBy(s => s.OrderNum))
                .ThenInclude(s => s.Questions)
                    .ThenInclude(q => q.Options);

        // Add any additional includes
        if (includeProperties != null)
        {
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
        }

        return await query
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
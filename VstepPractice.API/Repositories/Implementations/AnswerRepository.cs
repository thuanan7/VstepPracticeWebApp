using VstepPractice.API.Data;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;

namespace VstepPractice.API.Repositories.Implementations;

public class AnswerRepository : RepositoryBase<Answer, int>, IAnswerRepository
{
    public AnswerRepository(ApplicationDbContext context) : base(context)
    {
    }
}

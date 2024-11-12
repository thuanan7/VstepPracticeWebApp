using System.Linq.Expressions;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Repositories.Interfaces;

public interface IUserRepository : IRepositoryBase<User, int>
{
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
}

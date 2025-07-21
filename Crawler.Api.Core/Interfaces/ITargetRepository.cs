using Crawler.Api.Core.Entities;

namespace Crawler.Api.Core.Interfaces
{
    public interface ITargetRepository
    {
        Task<List<Target>> GetActiveTargetsBySocialNetworkAsync(string socialNetwork);
        Task<Target> GetByUsernameAsync(string username);
        Task AddAsync(Target target);
    }
}

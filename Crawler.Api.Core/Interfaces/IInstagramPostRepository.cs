using Crawler.Api.Core.Entities;

namespace Crawler.Api.Core.Interfaces
{
    public interface IInstagramPostRepository
    {
        // Um método para buscar um post pelo seu ID do Instagram, para evitar duplicatas.
        Task<InstagramPost> GetByPostIdAsync(string postId);

        // Um método para salvar uma lista de novos posts.
        Task AddManyAsync(IEnumerable<InstagramPost> posts);
        Task<List<InstagramPost>> GetPostsByUsernameAsync(string username);
    }
}

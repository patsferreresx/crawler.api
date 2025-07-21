using Crawler.Api.Core.Entities;
using Crawler.Api.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Crawler.Api.Infrastructure.Persistence.Repositories
{
    public class MongoDbPostRepository : IInstagramPostRepository
    {
        private readonly IMongoCollection<InstagramPost> _postsCollection;

        public MongoDbPostRepository(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            var databaseName = configuration["MongoDbSettings:DatabaseName"];
            var collectionName = configuration["MongoDbSettings:PostsCollectionName"];

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _postsCollection = database.GetCollection<InstagramPost>(collectionName);
        }

        public async Task AddManyAsync(IEnumerable<InstagramPost> posts)
        {
            await _postsCollection.InsertManyAsync(posts);
        }

        public async Task<InstagramPost> GetByPostIdAsync(string postId)
        {
            return await _postsCollection.Find(p => p.PostIdFromInstagram == postId).FirstOrDefaultAsync();
        }

        public async Task<List<InstagramPost>> GetPostsByUsernameAsync(string username)
        {
            // Busca todos os posts do usuário e ordena pelos mais recentes
            return await _postsCollection.Find(p => p.OwnerUsername == username)
                                         .SortByDescending(p => p.Timestamp)
                                         .ToListAsync();
        }
    }
}

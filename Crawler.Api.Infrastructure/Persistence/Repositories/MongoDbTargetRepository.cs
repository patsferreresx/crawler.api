using Crawler.Api.Core.Entities;
using Crawler.Api.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Crawler.Api.Infrastructure.Persistence.Repositories
{
    public class MongoDbTargetRepository : ITargetRepository
    {
        private readonly IMongoCollection<Target> _targetsCollection;

        public MongoDbTargetRepository(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            var databaseName = configuration["MongoDbSettings:DatabaseName"];
            var collectionName = configuration["MongoDbSettings:TargetsCollectionName"];

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _targetsCollection = database.GetCollection<Target>(collectionName);
        }

        public async Task AddAsync(Target target)
        {
            await _targetsCollection.InsertOneAsync(target);
        }

        public async Task<List<Target>> GetActiveTargetsBySocialNetworkAsync(string socialNetwork)
        {
            return await _targetsCollection.Find(t => t.SocialNetwork == socialNetwork && t.IsEnabled).ToListAsync();
        }

        public async Task<Target> GetByUsernameAsync(string username)
        {
            return await _targetsCollection.Find(t => t.Username == username).FirstOrDefaultAsync();
        }
    }
}

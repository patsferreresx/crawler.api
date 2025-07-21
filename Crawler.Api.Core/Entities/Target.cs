using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crawler.Api.Core.Entities
{
    public class Target
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("SocialNetwork")]
        public string SocialNetwork { get; set; }

        [BsonElement("MaxItems")]
        public int? MaxItems { get; set; }

        [BsonElement("IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

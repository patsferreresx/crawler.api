using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crawler.Api.Core.Entities
{
    public class InstagramPost
    {
        // Atributos do MongoDB para mapear o 'Id' para o '_id' do banco como um ObjectId.
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("PostId")] // Nome do campo no banco de dados
        public string PostIdFromInstagram { get; set; }

        [BsonElement("OwnerUsername")]
        public string OwnerUsername { get; set; }

        [BsonElement("Url")]
        public string Url { get; set; }

        [BsonElement("Caption")]
        public string Caption { get; set; }

        [BsonElement("LikesCount")]
        public int LikesCount { get; set; }

        [BsonElement("DisplayUrl")]
        public string DisplayUrl { get; set; } // URL da imagem/thumbnail

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("CrawledAt")]
        public DateTime CrawledAt { get; set; } = DateTime.UtcNow; // Data em que salvamos
    }
}

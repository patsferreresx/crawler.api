using System.Text.Json.Serialization;

namespace Crawler.Api.Infrastructure.DTOs
{
    public class ApifyInstagramPostDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("likesCount")]
        public int LikesCount { get; set; }

        [JsonPropertyName("displayUrl")]
        public string DisplayUrl { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("ownerUsername")]
        public string OwnerUsername { get; set; }
    }
}

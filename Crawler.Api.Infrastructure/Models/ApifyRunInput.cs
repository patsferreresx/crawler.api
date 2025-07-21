using System.Text.Json.Serialization;

namespace Crawler.Api.Infrastructure.Models
{
    internal class ApifyRunInput
    {
        public string[] DirectUrls { get; set; }
        public string ResultsType { get; set; } = "posts";
        public int ResultsLimit { get; set; }
        public bool ShouldCollectComments { get; set; } = false;
        public bool SkipPinnedPosts { get; set; } = true;

        // Propriedade opcional que só usamos para webhooks
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object[] Webhooks { get; set; }
    }
}

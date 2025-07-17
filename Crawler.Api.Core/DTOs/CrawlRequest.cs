namespace Crawler.Api.Core.DTOs
{
    public class CrawlRequest
    {
        public string TargetUsername { get; set; }
        public int? MaxItems { get; set; }

    }
}

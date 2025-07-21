namespace Crawler.Api.Core.Interfaces
{
    public interface ICrawlerService
    {
        Task<string> CrawlAndGetResultAsync(string targetUsername, int? maxItems);

        Task<string> StartCrawlWithWebhookAsync(string targetUsername, int? maxItems);
        Task<string> GetCrawlResultAsync(string datasetId);
    }
}

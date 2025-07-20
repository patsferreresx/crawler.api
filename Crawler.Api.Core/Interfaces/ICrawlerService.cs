namespace Crawler.Api.Core.Interfaces
{
    public interface ICrawlerService
    {
        Task<string> CrawlAndGetResultAsync(string targetUsername, int? maxItems);
    }
}

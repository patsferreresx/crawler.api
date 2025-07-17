namespace Crawler.Api.Core.Interfaces
{
    public interface ICrawlerService
    {
        Task<string> RunCrawlAsync(string targetUsername, int? maxItems);
    }
}

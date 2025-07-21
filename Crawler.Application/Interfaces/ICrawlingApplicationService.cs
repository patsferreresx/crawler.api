namespace Crawler.Application.Interfaces
{
    public interface ICrawlingApplicationService
    {
        // Um método que recebe o JSON bruto e faz todo o trabalho de salvar no banco.
        Task ProcessAndSaveCrawlResultAsync(string resultJson);
    }
}

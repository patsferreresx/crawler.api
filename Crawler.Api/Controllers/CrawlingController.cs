using Crawler.Api.Core.DTOs;
using Crawler.Api.Core.Interfaces;
using Crawler.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crawler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrawlingController : ControllerBase
    {
        private readonly ICrawlerService _crawlerService;
        private readonly ICrawlingApplicationService _appService;
        private readonly ILogger<CrawlingController> _logger;

        // Injetamos a interface do nosso serviço no construtor
        public CrawlingController(ICrawlerService crawlerService, ICrawlingApplicationService appService, ILogger<CrawlingController> logger)
        {
            _crawlerService = crawlerService;
            _appService = appService;
            _logger = logger;
        }

        // Endpoint para o USUÁRIO disparar um crawl
        [HttpPost("trigger")]
        public async Task<IActionResult> StartInstagramCrawl([FromBody] CrawlRequest request)
        {
            var runId = await _crawlerService.StartCrawlWithWebhookAsync(request.TargetUsername, request.MaxItems);
            return Accepted(new { Message = "Requisição de crawling aceita.", RunId = runId });
        }

        // Endpoint para o APIFY nos avisar que terminou
        [HttpPost("webhook-receiver")]
        public async Task<IActionResult> ApifyWebhookReceiver([FromBody] ApifyWebhookPayload payload)
        {
            _logger.LogInformation("Webhook recebido da Apify! Status: {Status}", payload.Resource.Status);

            if (payload.EventType == "ACTOR.RUN.SUCCEEDED")
            {
                var resultJson = await _crawlerService.GetCrawlResultAsync(payload.Resource.DefaultDatasetId);
                // Usamos nosso serviço de aplicação compartilhado para processar e salvar!
                await _appService.ProcessAndSaveCrawlResultAsync(resultJson);
            }

            return Ok();
        }
    }
}

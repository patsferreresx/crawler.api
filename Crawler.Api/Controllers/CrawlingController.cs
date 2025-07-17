using Crawler.Api.Core.DTOs;
using Crawler.Api.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crawler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrawlingController : ControllerBase
    {
        private readonly ICrawlerService _crawlerService;

        // Injetamos a interface do nosso serviço no construtor
        public CrawlingController(ICrawlerService crawlerService)
        {
            _crawlerService = crawlerService;
        }

        [HttpPost("instagram")]
        public async Task<IActionResult> StartInstagramCrawl([FromBody] CrawlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TargetUsername))
            {
                return BadRequest("O nome de usuário (TargetUsername) é obrigatório.");
            }

            try
            {
                // Chamamos o nosso serviço para executar o script Python
                string resultJson = await _crawlerService.RunCrawlAsync(request.TargetUsername, request.MaxItems);

                // Por enquanto, apenas retornamos o JSON bruto como resposta
                // No futuro, vamos desserializar e salvar no banco aqui
                return Content(resultJson, "application/json");
            }
            catch (Exception ex)
            {
                // Se o serviço der um erro (ex: script não encontrado, erro no python), retornamos um erro 500
                return StatusCode(500, new { Message = "Ocorreu um erro interno no servidor.", Error = ex.Message });
            }
        }
    }
}

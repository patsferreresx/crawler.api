using Crawler.Api.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Crawler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrawlingController : ControllerBase
    {
        // O atributo [HttpPost] define que este método responde a requisições POST.
        // O nome "instagram" será parte da URL: /api/crawling/instagram
        [HttpPost("instagram")]
        public IActionResult StartInstagramCrawl([FromBody] CrawlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TargetUsername))
            {
                return BadRequest("O nome de usuário (TargetUsername) é obrigatório.");
            }

            // Por enquanto, apenas confirmamos o recebimento.
            // Nos próximos passos, vamos chamar o worker Python aqui.
            var responseMessage = $"Requisição de crawling para o perfil '{request.TargetUsername}' recebida. Itens a buscar: {request.MaxItems ?? 10}.";

            return Ok(new { Message = responseMessage });
        }
    }
}

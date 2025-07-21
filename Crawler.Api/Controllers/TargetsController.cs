using Crawler.Api.Core.DTOs;
using Crawler.Api.Core.Entities;
using Crawler.Api.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Crawler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TargetsController : ControllerBase
    {
        private readonly ITargetRepository _targetRepository;
        private readonly ICrawlerService _crawlerService;
        private readonly IInstagramPostRepository _postRepository;
        private readonly ILogger<TargetsController> _logger;

        public TargetsController(ITargetRepository targetRepository, ICrawlerService crawlerService, IInstagramPostRepository postRepository, ILogger<TargetsController> logger)
        {
            _targetRepository = targetRepository;
            _crawlerService = crawlerService;
            _postRepository = postRepository;
            _logger = logger;
        }

        [HttpPost("instagram")]
        public async Task<IActionResult> AddInstagramTarget([FromBody] CreateTargetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest("O nome de usuário é obrigatório.");

            var existingTarget = await _targetRepository.GetByUsernameAsync(request.Username);

            if (existingTarget != null)
            {
                // CENÁRIO 1: O ALVO JÁ EXISTE
                _logger.LogInformation("Alvo '{Username}' já existe. Disparando atualização em background e retornando dados existentes.", request.Username);

                // Dispara o crawl em background para buscar atualizações (fire-and-forget)
                _ = _crawlerService.StartCrawlWithWebhookAsync(request.Username, request.MaxItems);

                // Busca os posts que já temos no banco para esse usuário
                var existingPosts = await _postRepository.GetPostsByUsernameAsync(request.Username);

                // Retorna 200 OK com os dados que já temos, para o front-end mostrar na hora
                return Ok(existingPosts);
            }
            else
            {
                // CENÁRIO 2: O ALVO É NOVO
                _logger.LogInformation("Cadastrando novo alvo '{Username}' e disparando crawl inicial.", request.Username);

                var newTarget = new Target
                {
                    Username = request.Username,
                    MaxItems = request.MaxItems,
                    SocialNetwork = "Instagram"
                };
                await _targetRepository.AddAsync(newTarget);

                var runId = await _crawlerService.StartCrawlWithWebhookAsync(newTarget.Username, newTarget.MaxItems);

                // Retorna 201 Created, pois um novo alvo foi criado.
                // O front-end saberá que precisa esperar os dados chegarem.
                return CreatedAtAction(nameof(GetTargetByUsername), new { username = newTarget.Username }, new
                {
                    Message = "Alvo cadastrado com sucesso. Crawling inicial em progresso.",
                    RunId = runId,
                    NewTarget = newTarget
                });
            }
        }

        // Endpoint auxiliar para buscar um alvo pelo nome
        [HttpGet("{username}")]
        public async Task<IActionResult> GetTargetByUsername(string username)
        {
            var target = await _targetRepository.GetByUsernameAsync(username);
            if (target == null)
                return NotFound();

            return Ok(target);
        }
    }
}

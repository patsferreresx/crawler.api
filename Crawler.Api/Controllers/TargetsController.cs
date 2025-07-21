using Crawler.Api.Core.DTOs;
using Crawler.Api.Core.Entities;
using Crawler.Api.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crawler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TargetsController : ControllerBase
    {
        private readonly ITargetRepository _targetRepository;

        public TargetsController(ITargetRepository targetRepository)
        {
            _targetRepository = targetRepository;
        }

        // Endpoint para cadastrar um novo alvo
        [HttpPost("instagram")]
        public async Task<IActionResult> AddInstagramTarget([FromBody] CreateTargetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest("O nome de usuário é obrigatório.");

            var existingTarget = await _targetRepository.GetByUsernameAsync(request.Username);
            if (existingTarget != null)
                return Conflict($"O alvo '{request.Username}' já está cadastrado.");

            var newTarget = new Target
            {
                Username = request.Username,
                MaxItems = request.MaxItems,
                SocialNetwork = "Instagram"
            };

            await _targetRepository.AddAsync(newTarget);

            // Retorna 201 Created com a localização do novo recurso (boa prática)
            return CreatedAtAction(nameof(GetTargetByUsername), new { username = newTarget.Username }, newTarget);
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

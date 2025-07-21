using Crawler.Api.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crawler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IInstagramPostRepository _postRepository;

        public PostsController(IInstagramPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        // GET /api/posts/leomessi
        [HttpGet("{username}")]
        public async Task<IActionResult> GetPostsByUsername(string username)
        {
            // Agora a chamada ao repositório funciona.
            var posts = await _postRepository.GetPostsByUsernameAsync(username);

            // Retorna 200 OK com a lista de posts (pode ser uma lista vazia, o que está correto).
            return Ok(posts);
        }
    }
}

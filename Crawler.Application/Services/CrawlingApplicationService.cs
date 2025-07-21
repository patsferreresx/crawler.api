using Crawler.Application.Interfaces;
using Crawler.Api.Core.Entities;
using Crawler.Api.Core.Interfaces;
using Crawler.Api.Infrastructure.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace Crawler.Application.Services
{
    public class CrawlingApplicationService : ICrawlingApplicationService
    {
        private readonly IInstagramPostRepository _postRepository;
        private readonly ILogger<CrawlingApplicationService> _logger;

        public CrawlingApplicationService(IInstagramPostRepository postRepository, ILogger<CrawlingApplicationService> logger)
        {
            _postRepository = postRepository;
            _logger = logger;
        }

        public async Task ProcessAndSaveCrawlResultAsync(string resultJson)
        {
            var apifyPosts = JsonSerializer.Deserialize<List<ApifyInstagramPostDto>>(resultJson);

            if (apifyPosts is null || !apifyPosts.Any())
            {
                _logger.LogInformation("Nenhum post retornado pela Apify para processar.");
                return;
            }

            var newPostsToSave = new List<InstagramPost>();
            foreach (var apifyPost in apifyPosts)
            {
                var existingPost = await _postRepository.GetByPostIdAsync(apifyPost.Id);
                if (existingPost is null)
                {
                    var cleanPost = new InstagramPost
                    {
                        PostIdFromInstagram = apifyPost.Id,
                        OwnerUsername = apifyPost.OwnerUsername,
                        Url = apifyPost.Url,
                        Caption = apifyPost.Caption,
                        LikesCount = apifyPost.LikesCount,
                        DisplayUrl = apifyPost.DisplayUrl,
                        Timestamp = apifyPost.Timestamp
                    };
                    newPostsToSave.Add(cleanPost);
                }
            }

            if (newPostsToSave.Any())
            {
                await _postRepository.AddManyAsync(newPostsToSave);
                _logger.LogInformation("{count} novos posts salvos no banco de dados.", newPostsToSave.Count);
            }
            else
            {
                _logger.LogInformation("Nenhum post novo encontrado para salvar.");
            }
        }

    }
}

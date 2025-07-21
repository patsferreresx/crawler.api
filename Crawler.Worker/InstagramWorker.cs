using Crawler.Api.Core.Entities;
using Crawler.Api.Core.Interfaces;
using Crawler.Api.Infrastructure.DTOs;
using Crawler.Worker.Models;
using Cronos;
using System.Text.Json;

namespace Crawler.Worker;

public class InstagramWorker : BackgroundService
{
    private readonly ILogger<InstagramWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public InstagramWorker(ILogger<InstagramWorker> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory; // Usamos a factory para criar um escopo para cada execução
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cronExpression = _configuration["InstagramJob:CronExpression"];
        if (string.IsNullOrEmpty(cronExpression) || !_configuration.GetValue<bool>("InstagramJob:IsEnabled"))
        {
            _logger.LogWarning("Worker do Instagram está desabilitado ou sem CronExpression configurada.");
            return;
        }

        var cron = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = DateTime.UtcNow;
            var nextUtc = cron.GetNextOccurrence(utcNow);

            if (!nextUtc.HasValue)
            {
                // Se não houver próxima ocorrência, encerra o worker.
                return;
            }

            if (nextUtc.HasValue)
            {
                var delay = nextUtc.Value - utcNow;

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation("Próxima execução do InstagramWorker em: {delay}", delay);
                    await Task.Delay(delay, stoppingToken);
                }
            }

            _logger.LogInformation("Iniciando ciclo do InstagramWorker: {time}", DateTimeOffset.Now);

            // Criamos um escopo para resolver os serviços. É uma boa prática em workers.
            using (var scope = _scopeFactory.CreateScope())
            {
                var crawlerService = scope.ServiceProvider.GetRequiredService<ICrawlerService>();

                var postRepository = scope.ServiceProvider.GetRequiredService<IInstagramPostRepository>();

                var targets = _configuration.GetSection("InstagramJob:Targets").Get<List<InstagramTarget>>();
                if (targets is null || !targets.Any())
                {
                    _logger.LogWarning("Nenhum alvo configurado para o InstagramJob.");
                    continue;
                }

                foreach (var target in targets)
                {
                    try
                    {
                        // 1. Buscamos o JSON bruto
                        string resultJson = await crawlerService.CrawlAndGetResultAsync(target.Username, target.MaxItems);

                        // 2. Desserializamos o JSON "sujo" para uma lista de DTOs
                        var apifyPosts = JsonSerializer.Deserialize<List<ApifyInstagramPostDto>>(resultJson);

                        if (apifyPosts is null || !apifyPosts.Any())
                        {
                            _logger.LogInformation("Nenhum post retornado pela Apify para o alvo {Username}", target.Username);
                            continue;
                        }

                        var newPostsToSave = new List<InstagramPost>();
                        foreach (var apifyPost in apifyPosts)
                        {
                            // 3. Verificamos se o post já existe no nosso banco para não duplicar
                            var existingPost = await postRepository.GetByPostIdAsync(apifyPost.Id);
                            if (existingPost is null)
                            {
                                // 4. Mapeamos do DTO "sujo" para nossa entidade "limpa"
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

                        // 5. Se tivermos novos posts, salvamos todos de uma vez
                        if (newPostsToSave.Any())
                        {
                            await postRepository.AddManyAsync(newPostsToSave);
                            _logger.LogInformation("{count} novos posts do alvo {Username} salvos no banco de dados.", newPostsToSave.Count, target.Username);
                        }
                        else
                        {
                            _logger.LogInformation("Nenhum post novo encontrado para o alvo {Username}.", target.Username);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Falha ao processar o alvo {Username}", target.Username);
                    }
                }
            }
        }
    }
}
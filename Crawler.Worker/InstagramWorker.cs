using Crawler.Api.Core.Interfaces;
using Crawler.Application.Interfaces;
using Crawler.Worker.Models;
using Cronos;

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
            _logger.LogWarning("Worker do Instagram está desabilitado.");
            return;
        }

        var cron = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = DateTime.UtcNow;
            var nextUtc = cron.GetNextOccurrence(utcNow);

            if (!nextUtc.HasValue) return;

            var delay = nextUtc.Value - utcNow;
            if (delay > TimeSpan.Zero)
            {
                _logger.LogInformation("Próxima execução do InstagramWorker em: {delay}", delay);
                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogWarning("================ INICIANDO NOVO CICLO DO WORKER ================ ({time})", DateTimeOffset.Now);

            using (var scope = _scopeFactory.CreateScope())
            {
                // Pegamos os serviços que precisamos dentro do escopo
                var targetRepository = scope.ServiceProvider.GetRequiredService<ITargetRepository>();
                var appService = scope.ServiceProvider.GetRequiredService<ICrawlingApplicationService>();

                //var targets = _configuration.GetSection("InstagramJob:Targets").Get<List<InstagramTarget>>();
                var targets = await targetRepository.GetActiveTargetsBySocialNetworkAsync("Instagram");

                if (targets is null || !targets.Any())
                {
                    _logger.LogWarning("Nenhum alvo configurado para o InstagramJob.");
                    continue;
                }

                _logger.LogInformation("Encontrados {count} alvos para processar.", targets.Count);

                foreach (var target in targets)
                {
                    // Usamos um "escopo de log" para que todas as mensagens dentro deste bloco
                    // tenham a informação do Username anexada.
                    using (_logger.BeginScope("Processando Alvo: {Username}", target.Username))
                    {
                        try
                        {
                            _logger.LogInformation("Buscando dados...");

                            var crawlerService = scope.ServiceProvider.GetRequiredService<ICrawlerService>();
                            string resultJson = await crawlerService.CrawlAndGetResultAsync(target.Username, target.MaxItems);
                            await appService.ProcessAndSaveCrawlResultAsync(resultJson);

                            _logger.LogInformation("Processamento do alvo concluído com sucesso.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Falha ao processar o alvo.");
                        }
                    }
                }
            }

            _logger.LogWarning("================ CICLO DO WORKER FINALIZADO ================");
        }
    }
}
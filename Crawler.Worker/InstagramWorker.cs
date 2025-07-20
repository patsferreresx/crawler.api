using Crawler.Api.Core.Interfaces;
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
                        _logger.LogInformation("Processando alvo: {Username}", target.Username);
                        string resultJson = await crawlerService.CrawlAndGetResultAsync(target.Username, target.MaxItems);

                        // TODO: Salvar o 'resultJson' no banco de dados.
                        _logger.LogInformation("Crawling do alvo {Username} concluído com sucesso.", target.Username);
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
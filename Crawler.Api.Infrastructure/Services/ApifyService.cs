using Crawler.Api.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Crawler.Api.Infrastructure.Services
{
    public class ApifyService : ICrawlerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApifyService> _logger;
        private readonly string _apifyToken;
        private readonly string _actorId;
        private readonly string _apifyBaseUrl;

        public ApifyService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApifyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            // Lemos as configurações uma vez no construtor
            _apifyToken = _configuration["ApifySettings:ApiToken"] ?? throw new ArgumentNullException("ApiToken");
            _actorId = _configuration["ApifySettings:ActorId"] ?? throw new ArgumentNullException("ActorId");
            _apifyBaseUrl = _configuration["ApifySettings:BaseUrl"] ?? "https://api.apify.com/v2";
        }

        // Este método implementa o ciclo completo: Iniciar -> Esperar (Polling) -> Retornar Resultado
        public async Task<string> CrawlAndGetResultAsync(string targetUsername, int? maxItems)
        {
            var client = _httpClientFactory.CreateClient();

            // 1. Iniciar a execução do Ator
            var startUrl = $"{_apifyBaseUrl}/acts/{_actorId}/runs?token={_apifyToken}";
            var runInput = new
            {
                directUrls = new[] { $"https://www.instagram.com/{targetUsername}/" },
                resultsType = "posts",
                resultsLimit = maxItems ?? 10,
                skipPinnedPosts = true,
                shouldCollectComments = false
            };

            _logger.LogInformation("Iniciando ator da Apify para o alvo: {target}", targetUsername);
            var response = await client.PostAsJsonAsync(startUrl, runInput);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
            var runId = responseData.GetProperty("data").GetProperty("id").GetString();
            _logger.LogInformation("Ator iniciado com sucesso. RunId: {runId}", runId);

            // 2. Esperar pelo resultado (Polling)
            var statusUrl = $"{_apifyBaseUrl}/actor-runs/{runId}?token={_apifyToken}";
            string status = "";
            JsonElement resultData = new();

            while (status != "SUCCEEDED" && status != "FAILED" && status != "TIMED-OUT")
            {
                await Task.Delay(5000); // Espera 5 segundos entre cada verificação
                var statusResponse = await client.GetFromJsonAsync<JsonElement>(statusUrl);
                resultData = statusResponse.GetProperty("data");
                status = resultData.GetProperty("status").GetString();
                _logger.LogInformation("Status atual do RunId {runId}: {status}", runId, status);
            }

            // 3. Verificar o resultado e buscar os dados
            if (status != "SUCCEEDED")
            {
                throw new InvalidOperationException($"O crawling falhou com o status: {status}");
            }

            var datasetId = resultData.GetProperty("defaultDatasetId").GetString();
            _logger.LogInformation("Crawling concluído. Buscando resultados do DatasetId: {datasetId}", datasetId);

            var itemsUrl = $"{_apifyBaseUrl}/datasets/{datasetId}/items?token={_apifyToken}&format=json";
            var resultJson = await client.GetStringAsync(itemsUrl);

            return resultJson;
        }
    }
}

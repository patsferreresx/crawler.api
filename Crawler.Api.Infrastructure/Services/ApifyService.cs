using Crawler.Api.Core.Interfaces;
using Crawler.Api.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
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
            _apifyToken = _configuration["ApifySettings:ApiToken"] ?? throw new ArgumentNullException("ApiToken");
            _actorId = _configuration["ApifySettings:ActorId"] ?? throw new ArgumentNullException("ActorId");
            _apifyBaseUrl = _configuration["ApifySettings:BaseUrl"] ?? "https://api.apify.com/v2";
        }

        public async Task<string> CrawlAndGetResultAsync(string targetUsername, int? maxItems)
        {
            var runInput = CreateBaseRunInput(targetUsername, maxItems);
            var runId = await StartActorRunAsync(runInput);
            _logger.LogInformation("Ator iniciado com sucesso (Polling). RunId: {runId}", runId);

            var resultData = await WaitForRunToFinishAsync(runId);
            var datasetId = resultData.GetProperty("defaultDatasetId").GetString();

            var client = CreateAuthenticatedClient();
            return await GetDatasetItemsAsync(client, datasetId);
        }

        public async Task<string> StartCrawlWithWebhookAsync(string targetUsername, int? maxItems)
        {
            var runInput = CreateBaseRunInput(targetUsername, maxItems);
            var webhookUrl = $"{_configuration["ApplicationSettings:PublicBaseUrl"]}/api/crawling/webhook-receiver";

            // Adicionamos o detalhe do webhook ao input base
            runInput.Webhooks = new[] { new { eventType = "ACTOR.RUN.SUCCEEDED", requestUrl = webhookUrl } };

            var runId = await StartActorRunAsync(runInput);
            _logger.LogInformation("Ator iniciado com sucesso (Webhook). RunId: {runId}", runId);

            return runId;
        }

        // MÉTODO PRIVADO PARA CRIAR O INPUT BASE (SEM REPETIÇÃO)
        private ApifyRunInput CreateBaseRunInput(string targetUsername, int? maxItems)
        {
            return new ApifyRunInput
            {
                DirectUrls = new[] { $"https://www.instagram.com/{targetUsername}/" },
                ResultsLimit = maxItems ?? 10
            };
        }

        // MÉTODO PRIVADO PARA INICIAR QUALQUER JOB (SEM REPETIÇÃO)
        private async Task<string> StartActorRunAsync(ApifyRunInput input)
        {
            var client = CreateAuthenticatedClient();
            var startUrl = $"{_apifyBaseUrl}/acts/{_actorId}/runs";

            var response = await client.PostAsJsonAsync(startUrl, input);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
            return responseData.GetProperty("data").GetProperty("id").GetString();
        }

        // O método de busca de resultados foi simplificado para receber apenas o ID
        public async Task<string> GetCrawlResultAsync(string datasetId)
        {
            var client = CreateAuthenticatedClient();
            return await GetDatasetItemsAsync(client, datasetId);
        }

        // Métodos privados auxiliares (sem alterações, mas agora mais focados)
        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apifyToken);
            return client;
        }

        private async Task<JsonElement> WaitForRunToFinishAsync(string runId)
        {
            var client = CreateAuthenticatedClient();
            var statusUrl = $"{_apifyBaseUrl}/actor-runs/{runId}";
            string status = "";
            JsonElement resultData = new();

            while (status != "SUCCEEDED" && status != "FAILED" && status != "TIMED-OUT")
            {
                await Task.Delay(5000);
                var statusResponse = await client.GetFromJsonAsync<JsonElement>(statusUrl);
                resultData = statusResponse.GetProperty("data");
                status = resultData.GetProperty("status").GetString();
                _logger.LogInformation("Status atual do RunId {runId}: {status}", runId, status);
            }

            if (status != "SUCCEEDED")
                throw new InvalidOperationException($"O crawling falhou com o status: {status}");

            return resultData;
        }

        private async Task<string> GetDatasetItemsAsync(HttpClient client, string datasetId)
        {
            var itemsUrl = $"{_apifyBaseUrl}/datasets/{datasetId}/items?format=json";
            return await client.GetStringAsync(itemsUrl);
        }
    }
}

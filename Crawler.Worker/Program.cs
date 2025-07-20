using Crawler.Api.Core.Interfaces;
using Crawler.Api.Infrastructure.Services;
using Crawler.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Registra os serviços necessários
builder.Services.AddHttpClient(); // Para o ApifyService funcionar
builder.Services.AddScoped<ICrawlerService, ApifyService>(); // Nosso serviço compartilhado
builder.Services.AddHostedService<InstagramWorker>(); // Registra nosso worker para rodar em background


var host = builder.Build();
host.Run();

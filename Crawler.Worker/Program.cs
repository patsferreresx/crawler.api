using Crawler.Api.Core.Interfaces;
using Crawler.Api.Infrastructure.Persistence.Repositories;
using Crawler.Api.Infrastructure.Services;
using Crawler.Application.Interfaces;
using Crawler.Application.Services;
using Crawler.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Registra os serviços necessários
builder.Services.AddHttpClient(); // Para o ApifyService funcionar
builder.Services.AddScoped<ICrawlerService, ApifyService>(); // Nosso serviço compartilhado
builder.Services.AddScoped<IInstagramPostRepository, MongoDbPostRepository>();
builder.Services.AddScoped<ICrawlingApplicationService, CrawlingApplicationService>();
builder.Services.AddHostedService<InstagramWorker>(); // Registra nosso worker para rodar em background


var host = builder.Build();
host.Run();

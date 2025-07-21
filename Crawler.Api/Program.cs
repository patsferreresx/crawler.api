using Crawler.Api.Core.Interfaces;
using Crawler.Api.Infrastructure.Persistence.Repositories;
using Crawler.Api.Infrastructure.Services;
using Crawler.Application.Interfaces;
using Crawler.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<ICrawlerService, ApifyService>();
builder.Services.AddScoped<IInstagramPostRepository, MongoDbPostRepository>();
builder.Services.AddScoped<ICrawlingApplicationService, CrawlingApplicationService>();
builder.Services.AddScoped<ITargetRepository, MongoDbTargetRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
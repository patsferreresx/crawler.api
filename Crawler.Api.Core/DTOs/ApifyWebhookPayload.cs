namespace Crawler.Api.Core.DTOs;

public record ApifyWebhookPayload
{
    public string EventType { get; init; }
    public Resource Resource { get; init; }
}
namespace Crawler.Api.Core.DTOs;

public record Resource
{
    public string Id { get; init; }
    public string Status { get; init; }
    public string DefaultDatasetId { get; init; }
}
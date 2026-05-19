using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Gabriel.Core.Configuration;
using Gabriel.Engine.Tools.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Tools.Web;

// IWebSearch implementation backed by the Brave Search API. Plain GET on
// /search?q=... + X-Subscription-Token header for auth. The named HttpClient
// is configured in DependencyInjection.AddInfrastructure so the BaseAddress +
// timeout + key header all live in one place.
public sealed class BraveWebSearch : IWebSearch
{
    public const string HttpClientName = "BraveSearch";

    private readonly IHttpClientFactory _httpFactory;
    private readonly BraveSearchOptions _options;
    private readonly ILogger<BraveWebSearch> _logger;

    public BraveWebSearch(
        IHttpClientFactory httpFactory,
        IOptions<BraveSearchOptions> options,
        ILogger<BraveWebSearch> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        if (!_options.IsConfigured)
        {
            throw new InvalidOperationException(
                "Brave Search API key is not configured. Set Tools:Web:Brave:ApiKey via Infisical or user-secrets.");
        }

        var http = _httpFactory.CreateClient(HttpClientName);
        var url = $"search?q={Uri.EscapeDataString(query)}&count={Math.Clamp(limit, 1, 10)}";

        using var response = await http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Brave Search failed: {Status} {Body}", (int)response.StatusCode, body);
            throw new HttpRequestException($"Brave Search returned {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<BraveSearchResponse>(cancellationToken: ct);
        var hits = payload?.Web?.Results ?? new List<BraveResult>();
        return hits.Select(h => new WebSearchResult(
            Title: h.Title ?? "",
            Url: h.Url ?? "",
            Snippet: h.Description ?? "")).ToList();
    }

    // Subset of the Brave Search response shape we actually consume.
    private sealed record BraveSearchResponse(
        [property: JsonPropertyName("web")] BraveWeb? Web);

    private sealed record BraveWeb(
        [property: JsonPropertyName("results")] List<BraveResult>? Results);

    private sealed record BraveResult(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("description")] string? Description);
}

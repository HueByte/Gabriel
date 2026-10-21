using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Gabriel.Core.Configuration;
using Gabriel.Engine.Tools.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Tools.Web;

// IWebSearch over a self-hosted SearXNG instance's JSON API:
// GET search?q=...&format=json&categories=general. SearXNG ships with the JSON
// format disabled, so the one failure worth explaining precisely is the 403 it
// returns until `json` is listed under search.formats in settings.yml (the
// compose stack's docker/searxng/settings.yml already does this).
public sealed class SearxngWebSearch : IWebSearch
{
    public const string HttpClientName = "SearxngSearch";

    private readonly IHttpClientFactory _httpFactory;
    private readonly SearxngSearchOptions _options;
    private readonly ILogger<SearxngWebSearch> _logger;

    public SearxngWebSearch(
        IHttpClientFactory httpFactory,
        IOptions<SearxngSearchOptions> options,
        ILogger<SearxngWebSearch> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        var http = _httpFactory.CreateClient(HttpClientName);
        var url = $"search?q={Uri.EscapeDataString(query)}&format=json&categories={Uri.EscapeDataString(_options.Categories)}";

        HttpResponseMessage response;
        try
        {
            response = await http.GetAsync(url, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(
                $"SearXNG at {http.BaseAddress} is unreachable ({ex.Message}). Start it with " +
                "`docker compose -f docker/docker-compose.yml up -d searxng`, or point Tools:Web:Searxng:BaseUrl at your instance.",
                ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "SearXNG refused format=json (403). Enable it by listing `json` under `search.formats` in the instance's settings.yml.");
            }
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("SearXNG search failed: {Status} {Body}", (int)response.StatusCode, body.Length > 300 ? body[..300] : body);
                throw new HttpRequestException($"SearXNG returned {(int)response.StatusCode}.");
            }

            var payload = await response.Content.ReadFromJsonAsync<SearxngResponse>(cancellationToken: ct);
            var results = payload?.Results ?? [];
            if (results.Count == 0 && payload?.UnresponsiveEngines is { Count: > 0 } down)
            {
                // Every upstream engine failing looks like "no results" otherwise.
                _logger.LogWarning("SearXNG returned no results; unresponsive engines: {Engines}",
                    string.Join(", ", down.Select(e => string.Join(':', e))));
            }

            return results
                .Where(r => !string.IsNullOrWhiteSpace(r.Url))
                .Take(Math.Clamp(limit, 1, 20))
                .Select(r => new WebSearchResult(r.Title ?? "", r.Url!, r.Content ?? ""))
                .ToList();
        }
    }

    // Subset of SearXNG's JSON response we consume.
    private sealed record SearxngResponse(
        [property: JsonPropertyName("results")] List<SearxngResult>? Results,
        [property: JsonPropertyName("unresponsive_engines")] List<List<string>>? UnresponsiveEngines);

    private sealed record SearxngResult(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("content")] string? Content);
}

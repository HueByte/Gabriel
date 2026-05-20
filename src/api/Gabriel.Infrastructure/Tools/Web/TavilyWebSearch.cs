using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Gabriel.Core.Configuration;
using Gabriel.Engine.Tools.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Tools.Web;

// IWebSearch backed by the Tavily Search API (tavily.com). POSTs a small JSON
// payload + Bearer token; the response carries an already-ranked list of
// {title, url, content, score} entries pre-trimmed for LLM consumption. Tavily
// is purpose-built for agents - their "content" field is roughly snippet-sized
// even on basic depth, so we don't have to re-truncate before handing back to
// the model.
public sealed class TavilyWebSearch : IWebSearch
{
    public const string HttpClientName = "TavilySearch";

    private readonly IHttpClientFactory _httpFactory;
    private readonly TavilySearchOptions _options;
    private readonly ILogger<TavilyWebSearch> _logger;

    public TavilyWebSearch(
        IHttpClientFactory httpFactory,
        IOptions<TavilySearchOptions> options,
        ILogger<TavilyWebSearch> logger)
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
                "Tavily Search API key is not configured. Set Tools:Web:Tavily:ApiKey via Infisical or user-secrets.");
        }

        var http = _httpFactory.CreateClient(HttpClientName);

        // Bearer token attached per-request rather than as a default header so
        // the HttpClient stays usable if the key ever rotates without an app
        // restart (Options.Value re-resolves to the current snapshot). Same
        // reason we pass api_key in the body alongside - Tavily accepts both
        // and either is sufficient.
        using var request = new HttpRequestMessage(HttpMethod.Post, "search")
        {
            Content = JsonContent.Create(new TavilySearchRequest(
                ApiKey: _options.ApiKey,
                Query: query,
                MaxResults: Math.Clamp(limit, 1, 20),
                SearchDepth: _options.SearchDepth,
                // We have our own web_fetch tool for full pages; raw_content
                // here would just bloat the response for no model-side gain.
                IncludeRawContent: false,
                IncludeImages: false,
                // Letting the model decide rather than pre-baking an answer
                // keeps the per-result citations intact for the reasoning step.
                IncludeAnswer: false)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Tavily Search failed: {Status} {Body}", (int)response.StatusCode, body);
            throw new HttpRequestException($"Tavily Search returned {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<TavilySearchResponse>(cancellationToken: ct);
        var hits = payload?.Results ?? new List<TavilyResult>();
        return hits.Select(h => new WebSearchResult(
            Title: h.Title ?? "",
            Url: h.Url ?? "",
            Snippet: h.Content ?? "")).ToList();
    }

    // Subset of the Tavily request shape we use. Property names match Tavily's
    // snake_case wire format via [JsonPropertyName].
    private sealed record TavilySearchRequest(
        [property: JsonPropertyName("api_key")] string ApiKey,
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("max_results")] int MaxResults,
        [property: JsonPropertyName("search_depth")] string SearchDepth,
        [property: JsonPropertyName("include_raw_content")] bool IncludeRawContent,
        [property: JsonPropertyName("include_images")] bool IncludeImages,
        [property: JsonPropertyName("include_answer")] bool IncludeAnswer);

    private sealed record TavilySearchResponse(
        [property: JsonPropertyName("results")] List<TavilyResult>? Results);

    private sealed record TavilyResult(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("score")] double? Score);
}

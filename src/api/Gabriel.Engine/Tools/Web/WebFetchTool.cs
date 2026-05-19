using System.Text;
using System.Text.Json;

namespace Gabriel.Engine.Tools.Web;

public sealed class WebFetchTool : ITool
{
    private readonly IUrlFetcher _fetcher;

    public WebFetchTool(IUrlFetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public string Name => "web_fetch";

    public string Description =>
        "Fetch and read the actual content of a public web page by URL. " +
        "USE THIS AFTER web_search when a result snippet looks relevant and you " +
        "need the full page text to answer the user — search snippets are short " +
        "and often miss the relevant detail. " +
        "Returns cleaned plain text (HTML tags stripped, script/style/nav removed, " +
        "whitespace normalized), capped at roughly 12,000 characters. " +
        "DO NOT use this for Gabriel-specific questions — use docs_read for those.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "url": {
              "type": "string",
              "description": "Absolute http(s) URL of the page to fetch. Get URLs from web_search results."
            }
          },
          "required": ["url"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("url", out var urlEl) || urlEl.ValueKind != JsonValueKind.String)
            return "Error: 'url' is required and must be a string.";
        var url = urlEl.GetString()!;
        if (string.IsNullOrWhiteSpace(url))
            return "Error: 'url' cannot be empty.";

        UrlFetchResult result;
        try
        {
            result = await _fetcher.FetchAsync(url, ct);
        }
        catch (Exception ex)
        {
            return $"Error: fetch failed — {ex.Message}";
        }

        var sb = new StringBuilder();
        sb.Append("Fetched: ").AppendLine(result.FinalUrl);
        sb.Append("Content-Type: ").AppendLine(result.ContentType);
        sb.Append("Length: ").Append(result.ContentLength).Append(" chars");
        if (result.Truncated) sb.Append(" (truncated — page was larger)");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine(result.Content);
        return sb.ToString();
    }
}

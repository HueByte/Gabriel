using System.Net;
using System.Text.RegularExpressions;
using Gabriel.Engine.Tools.Web;
using Microsoft.Extensions.Logging;

namespace Gabriel.Infrastructure.Tools.Web;

// Free, no-API-key IWebSearch backed by the DuckDuckGo HTML endpoint
// (https://html.duckduckgo.com/html/). We POST the query as form data, parse
// the result blocks out of the returned HTML, and unwrap the DDG redirect
// URLs (`/l/?uddg=ENCODED_URL`) to recover the destination.
//
// Tradeoffs vs. a paid API:
//   - No quota / no key — works out of the box.
//   - HTML may change shape; the parsing is regex-driven and intentionally
//     forgiving (parse failures yield zero results, not crashes).
//   - DuckDuckGo may rate-limit or block aggressive use. For a single-user
//     hobby deployment this is fine; production use should consider Brave.
public sealed class DuckDuckGoWebSearch : IWebSearch
{
    public const string HttpClientName = "DuckDuckGoSearch";

    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<DuckDuckGoWebSearch> _logger;

    public DuckDuckGoWebSearch(IHttpClientFactory httpFactory, ILogger<DuckDuckGoWebSearch> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        var http = _httpFactory.CreateClient(HttpClientName);
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("q", query),
            new KeyValuePair<string, string>("kl", "us-en"),
        });

        using var response = await http.PostAsync("html/", form, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("DuckDuckGo search returned {Status}", (int)response.StatusCode);
            throw new HttpRequestException($"DuckDuckGo HTML returned {(int)response.StatusCode}.");
        }

        var html = await response.Content.ReadAsStringAsync(ct);
        return Parse(html, limit);
    }

    // --- HTML parsing ---------------------------------------------------------

    // Each result is a <div class="result ..."> wrapping a title <a class="result__a">,
    // optional snippet <a class="result__snippet">, and a URL hint span. The structure
    // is generated server-side so the regex is stable enough for our use; if DDG
    // ever changes shape we fall back to zero results and log.
    private static readonly Regex ResultBlockRegex = new(
        @"<div[^>]*class=""[^""]*\bresult\b[^""]*""[^>]*>(?<body>.*?)(?=<div[^>]*class=""[^""]*\bresult\b|<div[^>]*id=""bottom_spacing""|\Z)",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex TitleLinkRegex = new(
        @"<a[^>]*class=""[^""]*\bresult__a\b[^""]*""[^>]*href=""(?<href>[^""]+)""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex SnippetRegex = new(
        @"class=""[^""]*\bresult__snippet\b[^""]*""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex TagStripRegex = new(@"<[^>]+>", RegexOptions.Compiled);

    private List<WebSearchResult> Parse(string html, int limit)
    {
        var results = new List<WebSearchResult>(limit);

        foreach (Match block in ResultBlockRegex.Matches(html))
        {
            if (results.Count >= limit) break;

            var body = block.Groups["body"].Value;

            var titleMatch = TitleLinkRegex.Match(body);
            if (!titleMatch.Success) continue;

            var rawHref = WebUtility.HtmlDecode(titleMatch.Groups["href"].Value);
            var title = CleanText(titleMatch.Groups["text"].Value);
            var url = UnwrapRedirect(rawHref);

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url)) continue;

            var snippetMatch = SnippetRegex.Match(body);
            var snippet = snippetMatch.Success ? CleanText(snippetMatch.Groups["text"].Value) : "";

            results.Add(new WebSearchResult(title, url, snippet));
        }

        if (results.Count == 0)
        {
            // Either zero genuine hits or the HTML format drifted. Log so we
            // notice before users complain — the tool layer will surface "no
            // results" to the model either way.
            _logger.LogInformation("DuckDuckGo returned no parseable results for the query");
        }

        return results;
    }

    // DDG wraps every destination in /l/?uddg=ENCODED_URL&rut=... — decode the
    // uddg parameter to recover the real target. Falls back to the raw href if
    // the wrapper shape is unrecognized.
    private static string UnwrapRedirect(string href)
    {
        const string marker = "uddg=";
        var idx = href.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0)
        {
            // DDG sometimes returns protocol-relative URLs (//example.com/...).
            if (href.StartsWith("//", StringComparison.Ordinal)) return "https:" + href;
            return href;
        }

        var start = idx + marker.Length;
        var end = href.IndexOf('&', start);
        var encoded = end < 0 ? href[start..] : href[start..end];
        try
        {
            return Uri.UnescapeDataString(encoded);
        }
        catch
        {
            return href;
        }
    }

    private static string CleanText(string raw)
        => TagStripRegex.Replace(WebUtility.HtmlDecode(raw), "").Trim();
}

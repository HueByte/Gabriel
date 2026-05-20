using System.Net;
using System.Text.RegularExpressions;
using Gabriel.Engine.Tools.Web;
using Microsoft.Extensions.Logging;

namespace Gabriel.Infrastructure.Tools.Web;

// Free, no-API-key IWebSearch backed by DuckDuckGo. We try two endpoints in
// order: the rich `html/` endpoint first (snippets, more metadata), and if
// that comes back with zero parseable results we fall back to the bare-bones
// `lite/` endpoint. Both POST the same form payload; the lite endpoint is
// designed for accessibility / low-bandwidth clients and gets bot-flagged
// far less aggressively, so it's a useful safety net for the case where
// DDG decides our traffic looks anomalous and returns a results-stripped
// page on `html/` while still answering the same query normally on `lite/`.
//
// Tradeoffs vs. a paid API:
//   - No quota / no key - works out of the box.
//   - HTML may change shape; the parsing is regex-driven and intentionally
//     forgiving (parse failures yield zero results, not crashes).
//   - DuckDuckGo may rate-limit or return an "anomaly" page (CAPTCHA-style)
//     to aggressive traffic; we detect that and log it with diagnostic
//     detail so failures are at least debuggable instead of silently
//     returning "no results". For production-scale use, Brave (with an API
//     key) is the more reliable backend.
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

        // Primary: rich html/ endpoint. Has snippets + redirect-wrapped URLs.
        var primary = await FetchAsync(http, "html/", query, ct);
        if (DetectAnomalyPage(primary))
        {
            _logger.LogWarning(
                "DuckDuckGo html/ returned an anomaly/block page for query '{Query}' (html length {Length}). Falling back to lite/.",
                query, primary.Length);
        }
        else
        {
            var parsed = ParseHtmlEndpoint(primary, limit);
            if (parsed.Count > 0) return parsed;
            _logger.LogInformation(
                "DuckDuckGo html/ returned 0 parseable results for query '{Query}' (html length {Length}). Falling back to lite/. First 200 chars: {Snippet}",
                query, primary.Length, FirstChars(primary, 200));
        }

        // Fallback: lite/ endpoint. Simpler markup, less aggressive bot-flagging.
        var lite = await FetchAsync(http, "lite/", query, ct);
        if (DetectAnomalyPage(lite))
        {
            _logger.LogWarning(
                "DuckDuckGo lite/ also returned an anomaly/block page for query '{Query}' (html length {Length}). Surfacing as zero results.",
                query, lite.Length);
            return Array.Empty<WebSearchResult>();
        }

        var liteParsed = ParseLiteEndpoint(lite, limit);
        if (liteParsed.Count == 0)
        {
            _logger.LogInformation(
                "DuckDuckGo lite/ also returned 0 parseable results for query '{Query}' (html length {Length}). Either DDG genuinely has nothing, or its HTML shape drifted. First 200 chars: {Snippet}",
                query, lite.Length, FirstChars(lite, 200));
        }
        return liteParsed;
    }

    private async Task<string> FetchAsync(HttpClient http, string path, string query, CancellationToken ct)
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("q", query),
            new KeyValuePair<string, string>("kl", "us-en"),
        });

        using var response = await http.PostAsync(path, form, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("DuckDuckGo {Path} returned {Status}", path, (int)response.StatusCode);
            throw new HttpRequestException($"DuckDuckGo {path} returned {(int)response.StatusCode}.");
        }
        return await response.Content.ReadAsStringAsync(ct);
    }

    // DDG ships a CAPTCHA-style "anomaly" page when it flags traffic; the body
    // mentions the anomaly modal explicitly. Catch it so we can fall back to a
    // sibling endpoint instead of treating it as "user query has no results".
    private static bool DetectAnomalyPage(string html)
        => html.Contains("anomaly_modal", StringComparison.Ordinal)
        || html.Contains("anomaly-modal", StringComparison.Ordinal)
        || html.Contains("anomaly.js", StringComparison.Ordinal);

    private static string FirstChars(string s, int n)
        => s.Length <= n ? s : s[..n];

    // --- HTML endpoint parsing ------------------------------------------------

    // Each result is a <div class="result ..."> wrapping a title <a class="result__a">,
    // optional snippet <a class="result__snippet">, and a URL hint span. The structure
    // is generated server-side so the regex is stable enough for our use; if DDG
    // ever changes shape we fall back to zero results and let the caller try lite/.
    private static readonly Regex HtmlResultBlockRegex = new(
        @"<div[^>]*class=""[^""]*\bresult\b[^""]*""[^>]*>(?<body>.*?)(?=<div[^>]*class=""[^""]*\bresult\b|<div[^>]*id=""bottom_spacing""|\Z)",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex HtmlTitleLinkRegex = new(
        @"<a[^>]*class=""[^""]*\bresult__a\b[^""]*""[^>]*href=""(?<href>[^""]+)""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex HtmlSnippetRegex = new(
        @"class=""[^""]*\bresult__snippet\b[^""]*""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex TagStripRegex = new(@"<[^>]+>", RegexOptions.Compiled);

    private static List<WebSearchResult> ParseHtmlEndpoint(string html, int limit)
    {
        var results = new List<WebSearchResult>(limit);

        foreach (Match block in HtmlResultBlockRegex.Matches(html))
        {
            if (results.Count >= limit) break;

            var body = block.Groups["body"].Value;

            var titleMatch = HtmlTitleLinkRegex.Match(body);
            if (!titleMatch.Success) continue;

            var rawHref = WebUtility.HtmlDecode(titleMatch.Groups["href"].Value);
            var title = CleanText(titleMatch.Groups["text"].Value);
            var url = UnwrapRedirect(rawHref);

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url)) continue;

            var snippetMatch = HtmlSnippetRegex.Match(body);
            var snippet = snippetMatch.Success ? CleanText(snippetMatch.Groups["text"].Value) : "";

            results.Add(new WebSearchResult(title, url, snippet));
        }

        return results;
    }

    // --- Lite endpoint parsing ------------------------------------------------

    // Lite endpoint uses single-quoted class attributes and a flatter table
    // structure: <a ... class='result-link'>title</a> followed (one row down)
    // by <td class='result-snippet'>snippet</td>. Links here aren't wrapped in
    // /l/?uddg=… for direct hits, but we still pass them through UnwrapRedirect
    // for safety - the helper is a no-op when the marker is absent.
    private static readonly Regex LiteResultLinkRegex = new(
        @"<a[^>]*href=""(?<href>[^""]+)""[^>]*class='result-link'[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex LiteSnippetRegex = new(
        @"<td[^>]*class='result-snippet'[^>]*>(?<text>.*?)</td>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static List<WebSearchResult> ParseLiteEndpoint(string html, int limit)
    {
        var linkMatches = LiteResultLinkRegex.Matches(html);
        var snippetMatches = LiteSnippetRegex.Matches(html);

        var results = new List<WebSearchResult>(limit);
        for (var i = 0; i < linkMatches.Count && results.Count < limit; i++)
        {
            var link = linkMatches[i];
            var rawHref = WebUtility.HtmlDecode(link.Groups["href"].Value);
            var title = CleanText(link.Groups["text"].Value);
            var url = UnwrapRedirect(rawHref);
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url)) continue;

            // Snippets and links share an index in the lite layout (one snippet
            // per link, in document order); if a result has no snippet the
            // index can still drift, but worst case the snippet is wrong - the
            // url/title are always correct.
            var snippet = i < snippetMatches.Count
                ? CleanText(snippetMatches[i].Groups["text"].Value)
                : "";

            results.Add(new WebSearchResult(title, url, snippet));
        }

        return results;
    }

    // DDG wraps every destination in /l/?uddg=ENCODED_URL&rut=... - decode the
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

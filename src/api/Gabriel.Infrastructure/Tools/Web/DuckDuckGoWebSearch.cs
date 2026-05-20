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

    // The two DDG search endpoints live on separate subdomains:
    //   https://html.duckduckgo.com/html/   - rich HTML output (the primary)
    //   https://lite.duckduckgo.com/lite/   - minimal HTML output (the fallback)
    // Using absolute URLs here so the request unambiguously hits the right
    // host regardless of what BaseAddress the named HttpClient was configured
    // with. (Older versions of this file pointed both at html.duckduckgo.com,
    // which silently broke the lite/ fallback - html.duckduckgo.com doesn't
    // serve the lite layout.)
    private const string HtmlEndpoint = "https://html.duckduckgo.com/html/";
    private const string LiteEndpoint = "https://lite.duckduckgo.com/lite/";

    public async Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        var http = _httpFactory.CreateClient(HttpClientName);

        // Primary: rich html/ endpoint. Has snippets + redirect-wrapped URLs.
        var primary = await FetchAsync(http, HtmlEndpoint, query, ct);
        var primaryBlocked = DetectAnomalyPage(primary);
        if (primaryBlocked)
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
        var lite = await FetchAsync(http, LiteEndpoint, query, ct);
        var liteBlocked = DetectAnomalyPage(lite);
        if (liteBlocked)
        {
            _logger.LogWarning(
                "DuckDuckGo lite/ also returned an anomaly/block page for query '{Query}' (html length {Length}). First 200 chars: {Snippet}",
                query, lite.Length, FirstChars(lite, 200));
        }
        else
        {
            var liteParsed = ParseLiteEndpoint(lite, limit);
            if (liteParsed.Count > 0) return liteParsed;
            _logger.LogInformation(
                "DuckDuckGo lite/ returned 0 parseable results for query '{Query}' (html length {Length}). First 200 chars: {Snippet}",
                query, lite.Length, FirstChars(lite, 200));
        }

        // Both endpoints exhausted. If at least one came back as an anomaly /
        // bot-block page, throwing with an actionable hint is more useful to
        // the agent than a silent empty-results return - the user almost
        // certainly needs a real API key. CompositeWebSearch swallows this
        // when other providers are configured, so multi-provider setups still
        // benefit from the other backends.
        if (primaryBlocked || liteBlocked)
        {
            throw new InvalidOperationException(
                "DuckDuckGo blocked this request as bot traffic (anomaly/CAPTCHA page returned on both html/ and lite/ endpoints). " +
                "DDG's free scraping endpoints rate-limit residential/datacenter IPs aggressively. " +
                "For reliable web search, set Tools__Web__Active=brave (or tavily) and supply the matching API key " +
                "via Tools__Web__Brave__ApiKey (or Tools__Web__Tavily__ApiKey).");
        }

        // Both endpoints returned 200 + parseable HTML + zero results. Genuine
        // empty result for this query - hand back [] like any backend would.
        return Array.Empty<WebSearchResult>();
    }

    private async Task<string> FetchAsync(HttpClient http, string url, string query, CancellationToken ct)
    {
        // GET (with query string) instead of POST (with form body): closer to
        // a real navigation, and html.duckduckgo.com / lite.duckduckgo.com
        // both serve the same content for either verb but bot-flag POST
        // requests more aggressively. Keeps Sec-Fetch-* headers consistent
        // with "user typed a URL and pressed enter" framing.
        var fullUrl = $"{url}?q={Uri.EscapeDataString(query)}&kl=us-en";

        using var response = await http.GetAsync(fullUrl, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("DuckDuckGo {Url} returned {Status}", url, (int)response.StatusCode);
            throw new HttpRequestException($"DuckDuckGo {url} returned {(int)response.StatusCode}.");
        }
        return await response.Content.ReadAsStringAsync(ct);
    }

    // DDG (and the Cloudflare layer in front of it) ships a few different
    // bot-block / CAPTCHA-style pages. We catch the markers from all of them
    // so the caller can fall back to a sibling endpoint or surface a clear
    // diagnostic instead of treating the block as "user query has no results".
    private static bool DetectAnomalyPage(string html)
        => html.Contains("anomaly_modal", StringComparison.Ordinal)
        || html.Contains("anomaly-modal", StringComparison.Ordinal)
        || html.Contains("anomaly.js", StringComparison.Ordinal)
        // Cloudflare "Just a moment..." interstitial.
        || html.Contains("Just a moment", StringComparison.Ordinal)
        || html.Contains("cf-mitigated", StringComparison.Ordinal)
        || html.Contains("cf-browser-verification", StringComparison.Ordinal)
        || html.Contains("__cf_chl_", StringComparison.Ordinal);

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

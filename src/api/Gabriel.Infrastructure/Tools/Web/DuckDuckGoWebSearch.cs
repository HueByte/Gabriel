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

    // Session state. The CookieContainer lives on the HttpClientHandler (see
    // DependencyInjection.ConfigureDdgHttpClient); we just track whether the
    // homepage pre-warm has already populated it for the current handler
    // generation, plus the User-Agent we committed to for that session.
    // The lock dedupes concurrent first-use callers so we don't pre-warm
    // twice in parallel.
    private readonly SemaphoreSlim _sessionLock = new(1, 1);
    private bool _sessionWarmed;
    private string? _sessionUserAgent;

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
    private const string Homepage = "https://duckduckgo.com/";

    // Small pool of recent real-browser User-Agent strings. We don't rotate
    // PER REQUEST - real browsers don't either, and rapid UA flipping within
    // a session is itself a bot tell. We pick one UA at session warmup and
    // hold it until anomaly detection resets the session. The pool just
    // spreads fingerprints across deployments / restarts.
    private static readonly string[] UserAgents =
    [
        // Chrome on Windows
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
        // Firefox on Windows
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0",
        // Edge on Windows
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0",
        // Chrome on macOS
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
    ];

    public async Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        var http = _httpFactory.CreateClient(HttpClientName);

        await EnsureSessionAsync(http, ct);

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
        // bot-block page, the cookies + UA we're using are burnt. Clear the
        // session state so the next call re-warms with a different UA and a
        // fresh cookie jar (DDG sometimes lifts the block once you stop
        // hammering with the flagged fingerprint).
        if (primaryBlocked || liteBlocked)
        {
            ResetSession();
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

    // One-time GET of the DDG homepage per session. Real browsers always
    // navigate to duckduckgo.com first (typing in the address bar / clicking
    // a bookmark), get a few session cookies, THEN submit a query. Cold
    // requests straight to /html/?q=... are one of the heuristics DDG flags
    // on. The cookies set here are scoped to .duckduckgo.com so they apply
    // to both html.* and lite.* subdomains automatically.
    private async Task EnsureSessionAsync(HttpClient http, CancellationToken ct)
    {
        if (_sessionWarmed) return;

        await _sessionLock.WaitAsync(ct);
        try
        {
            if (_sessionWarmed) return;

            // Commit to a UA for this session. Real browsers don't flip UA
            // between page loads, and rapid UA churn within the same cookie
            // jar reads as scripted.
            _sessionUserAgent = UserAgents[Random.Shared.Next(UserAgents.Length)];

            using var msg = BuildRequest(HttpMethod.Get, Homepage, isInitialNavigation: true);
            try
            {
                using var response = await http.SendAsync(msg, ct);
                // We don't care about the body - the value is in the cookies
                // the CookieContainer collected as a side effect of this
                // round-trip. Even an anomaly page on the homepage drops the
                // tracking cookies, so a 200 isn't required.
                _logger.LogDebug(
                    "DuckDuckGo session warmed via {Url} (status {Status}, UA {Ua}).",
                    Homepage, (int)response.StatusCode, _sessionUserAgent);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A failed warm-up is non-fatal - the actual search request
                // will still go through, just without the homepage cookies.
                // Log so we can see if warm-ups are systematically failing.
                _logger.LogWarning(ex, "DuckDuckGo homepage pre-warm failed; continuing without session cookies.");
            }

            _sessionWarmed = true;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    // Called when DDG hands us an anomaly page. Drops the warmed flag and
    // the chosen UA so the next SearchAsync rebuilds the session with a
    // freshly-picked UA and triggers another homepage round-trip - that
    // refreshes the cookie jar with whatever DDG hands back this time, and
    // shifts our fingerprint slightly.
    private void ResetSession()
    {
        _sessionWarmed = false;
        _sessionUserAgent = null;
    }

    private async Task<string> FetchAsync(HttpClient http, string url, string query, CancellationToken ct)
    {
        // Small randomized delay before the request lands. Tight back-to-back
        // requests at the millisecond level are one of the easier
        // bot-detection signals to pick up on; 200-1200ms covers the cadence
        // a real reader actually produces between submit-and-results.
        await Task.Delay(Random.Shared.Next(200, 1200), ct);

        // GET (with query string) instead of POST (with form body): closer to
        // a real navigation, and html.duckduckgo.com / lite.duckduckgo.com
        // both serve the same content for either verb but bot-flag POST
        // requests more aggressively. Keeps Sec-Fetch-* headers consistent
        // with "user typed a URL and pressed enter" framing.
        var fullUrl = $"{url}?q={Uri.EscapeDataString(query)}&kl=us-en";

        using var msg = BuildRequest(HttpMethod.Get, fullUrl, isInitialNavigation: false);
        using var response = await http.SendAsync(msg, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("DuckDuckGo {Url} returned {Status}", url, (int)response.StatusCode);
            throw new HttpRequestException($"DuckDuckGo {url} returned {(int)response.StatusCode}.");
        }
        return await response.Content.ReadAsStringAsync(ct);
    }

    // Builds an HttpRequestMessage with the full set of headers a real
    // Chrome/Firefox navigation carries. We construct per-request rather
    // than rely on HttpClient.DefaultRequestHeaders so the UA can be
    // session-pinned (and rotated on session reset) and Sec-Fetch-Site /
    // Referer can vary correctly between the "entering the site" pre-warm
    // and the subsequent "navigating within the site" search hop.
    private HttpRequestMessage BuildRequest(HttpMethod method, string url, bool isInitialNavigation)
    {
        var msg = new HttpRequestMessage(method, url);
        // UA falls back to the first pool entry if the session field is null
        // (defensive - EnsureSessionAsync should always have populated it
        // before a fetch reaches this code path).
        msg.Headers.Add("User-Agent", _sessionUserAgent ?? UserAgents[0]);
        msg.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        msg.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        msg.Headers.Add("DNT", "1");
        msg.Headers.Add("Upgrade-Insecure-Requests", "1");
        msg.Headers.Add("Sec-Fetch-Dest", "document");
        msg.Headers.Add("Sec-Fetch-Mode", "navigate");
        msg.Headers.Add("Sec-Fetch-User", "?1");
        // Initial navigation (warmup): no referrer, Sec-Fetch-Site: none -
        // the "user typed in address bar / opened a bookmark" signal.
        // Subsequent search: came from the homepage, same-site within
        // *.duckduckgo.com.
        if (isInitialNavigation)
        {
            msg.Headers.Add("Sec-Fetch-Site", "none");
        }
        else
        {
            msg.Headers.Add("Sec-Fetch-Site", "same-site");
            msg.Headers.Add("Referer", Homepage);
        }
        return msg;
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

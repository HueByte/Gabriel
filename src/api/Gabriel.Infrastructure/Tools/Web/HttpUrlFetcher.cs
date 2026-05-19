using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Gabriel.Engine.Tools.Web;
using Microsoft.Extensions.Logging;

namespace Gabriel.Infrastructure.Tools.Web;

// IUrlFetcher backed by HttpClient. Three concerns this class is built around:
//
//   1. SSRF defense — refuse non-HTTP(S) schemes and refuse hosts that resolve
//      to loopback / link-local / RFC1918 private ranges. Otherwise the agent
//      could be tricked into hitting the host's own metadata service
//      (169.254.169.254), an internal admin port, or a database.
//
//   2. Size cap — read up to a fixed byte ceiling so a giant page doesn't
//      blow the model's context window. Truncation is reported, not silent.
//
//   3. HTML→text — strip script/style/nav/header/footer first (their content
//      is rarely the answer), then strip remaining tags, decode entities,
//      collapse whitespace. Gets the readable body into the model's hands
//      without the page chrome.
public sealed class HttpUrlFetcher : IUrlFetcher
{
    public const string HttpClientName = "WebFetch";

    // 12k chars roughly = 3k tokens — a reasonable upper bound that leaves room
    // for the rest of the conversation in the model's context.
    private const int MaxContentChars = 12_000;
    // Read at most this many bytes from the wire before we stop; some pages
    // are megabytes of HTML around a few paragraphs of text.
    private const int MaxBytesToRead = 1_500_000;

    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<HttpUrlFetcher> _logger;

    public HttpUrlFetcher(IHttpClientFactory httpFactory, ILogger<HttpUrlFetcher> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<UrlFetchResult> FetchAsync(string url, CancellationToken ct)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("URL must be an absolute http(s) URL.");
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException($"Only http and https schemes are allowed; got '{uri.Scheme}'.");

        await AssertPublicHostAsync(uri.Host, ct);

        var http = _httpFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,text/plain;q=0.9,*/*;q=0.5");

        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Upstream returned {(int)response.StatusCode}.");

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        if (!IsTextLike(contentType))
            throw new InvalidOperationException($"Refusing to fetch non-text content (Content-Type: {contentType}).");

        // Bounded read — at most MaxBytesToRead bytes before we cut the stream.
        var bytes = await ReadBoundedAsync(response.Content, MaxBytesToRead, ct);
        var wasTruncatedAtBytes = bytes.Length >= MaxBytesToRead;

        var charset = response.Content.Headers.ContentType?.CharSet ?? "utf-8";
        string raw;
        try { raw = System.Text.Encoding.GetEncoding(charset).GetString(bytes); }
        catch { raw = System.Text.Encoding.UTF8.GetString(bytes); }

        var cleaned = contentType.Contains("html", StringComparison.OrdinalIgnoreCase)
            ? CleanHtml(raw)
            : CollapseWhitespace(raw);

        var truncatedAtChars = false;
        if (cleaned.Length > MaxContentChars)
        {
            cleaned = cleaned[..MaxContentChars] + "\n…[truncated]";
            truncatedAtChars = true;
        }

        return new UrlFetchResult(
            FinalUrl: response.RequestMessage?.RequestUri?.ToString() ?? uri.ToString(),
            ContentType: contentType,
            Content: cleaned,
            Truncated: wasTruncatedAtBytes || truncatedAtChars,
            ContentLength: cleaned.Length);
    }

    // --- SSRF guard ------------------------------------------------------------

    // Resolve the host and refuse if ANY resolved address falls in a private /
    // loopback / link-local / unspecified range. Catches both literal IP URLs
    // (http://127.0.0.1, http://169.254.169.254) and hostnames that DNS-route
    // to internal targets.
    private static async Task AssertPublicHostAsync(string host, CancellationToken ct)
    {
        IPAddress[] addresses;
        try { addresses = await Dns.GetHostAddressesAsync(host, ct); }
        catch (Exception ex) { throw new ArgumentException($"Host could not be resolved: {host} ({ex.Message})"); }

        foreach (var addr in addresses)
        {
            if (IsPrivate(addr))
                throw new ArgumentException($"Refusing to fetch a private/internal address ({addr}).");
        }
    }

    private static bool IsPrivate(IPAddress addr)
    {
        if (IPAddress.IsLoopback(addr)) return true;
        if (addr.Equals(IPAddress.Any) || addr.Equals(IPAddress.IPv6Any)) return true;

        if (addr.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = addr.GetAddressBytes();
            // 10.0.0.0/8
            if (b[0] == 10) return true;
            // 172.16.0.0/12
            if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true;
            // 192.168.0.0/16
            if (b[0] == 192 && b[1] == 168) return true;
            // 169.254.0.0/16 — link-local; AWS / GCP metadata lives here
            if (b[0] == 169 && b[1] == 254) return true;
            // 127.0.0.0/8 — already caught by IsLoopback for 127.0.0.1 but cover the rest
            if (b[0] == 127) return true;
            // 0.0.0.0/8 — unspecified
            if (b[0] == 0) return true;
            // 100.64.0.0/10 — CGNAT
            if (b[0] == 100 && b[1] >= 64 && b[1] <= 127) return true;
        }
        else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (addr.IsIPv6LinkLocal || addr.IsIPv6SiteLocal) return true;
            // Unique local fc00::/7
            var b = addr.GetAddressBytes();
            if ((b[0] & 0xFE) == 0xFC) return true;
        }
        return false;
    }

    // --- byte read with cap ----------------------------------------------------

    private static async Task<byte[]> ReadBoundedAsync(HttpContent content, int cap, CancellationToken ct)
    {
        await using var stream = await content.ReadAsStreamAsync(ct);
        using var memory = new MemoryStream();
        var buffer = new byte[8192];
        var total = 0;
        while (total < cap)
        {
            var remaining = cap - total;
            var toRead = Math.Min(buffer.Length, remaining);
            var read = await stream.ReadAsync(buffer.AsMemory(0, toRead), ct);
            if (read == 0) break;
            memory.Write(buffer, 0, read);
            total += read;
        }
        return memory.ToArray();
    }

    // --- HTML → text -----------------------------------------------------------

    // Strip the structural chrome that's rarely the answer, then strip remaining
    // tags. The order matters: we drop <script> and <style> bodies FIRST so the
    // generic tag-stripper doesn't leave their text contents dangling.
    private static readonly Regex DropBlockRegex = new(
        @"<(script|style|nav|header|footer|aside|noscript|svg)[^>]*>.*?</\1\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex CommentRegex = new(
        @"<!--.*?-->",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex TagRegex = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    private static readonly Regex WhitespaceRegex = new(
        @"[ \t\f\v]+",
        RegexOptions.Compiled);

    private static readonly Regex NewlineRunRegex = new(
        @"\r?\n\s*\r?\n+",
        RegexOptions.Compiled);

    private static string CleanHtml(string html)
    {
        var s = CommentRegex.Replace(html, "");
        s = DropBlockRegex.Replace(s, "");
        s = TagRegex.Replace(s, " ");
        s = WebUtility.HtmlDecode(s);
        return CollapseWhitespace(s);
    }

    private static string CollapseWhitespace(string s)
    {
        s = WhitespaceRegex.Replace(s, " ");
        s = NewlineRunRegex.Replace(s, "\n\n");
        return s.Trim();
    }

    private static bool IsTextLike(string contentType)
    {
        contentType = contentType.ToLowerInvariant();
        return contentType.StartsWith("text/")
            || contentType.Contains("xml")
            || contentType.Contains("json")
            || contentType.Contains("javascript");
    }
}

# DuckDuckGoWebSearch.cs

> **Source:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`

## Contents

- [DuckDuckGoWebSearch_overview](#duckduckgowebsearch_overview)
- [DuckDuckGoWebSearch](#duckduckgowebsearch)
- [BuildRequest](#buildrequest)
- [CleanText](#cleantext)
- [DetectAnomalyPage](#detectanomalypage)
- [EnsureSessionAsync](#ensuresessionasync)
- [FetchAsync](#fetchasync)
- [FirstChars](#firstchars)
- [ParseHtmlEndpoint](#parsehtmlendpoint)
- [ParseLiteEndpoint](#parseliteendpoint)
- [ResetSession](#resetsession)
- [SearchAsync](#searchasync)
- [UnwrapRedirect](#unwrapredirect)
- [Homepage](#homepage)
- [HtmlEndpoint](#htmlendpoint)
- [HtmlResultBlockRegex](#htmlresultblockregex)
- [HtmlSnippetRegex](#htmlsnippetregex)
- [HtmlTitleLinkRegex](#htmltitlelinkregex)
- [HttpClientName](#httpclientname)
- [LiteEndpoint](#liteendpoint)
- [LiteResultLinkRegex](#literesultlinkregex)
- [LiteSnippetRegex](#litesnippetregex)
- [TagStripRegex](#tagstripregex)
- [UserAgents](#useragents)

---

## DuckDuckGoWebSearch_overview

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** class

Implements IWebSearch using DuckDuckGo's public web endpoints (html/ then lite/ as a fallback). Reach for this when you need a zero‑configuration, no‑API‑key web search backend for development, demos, or light/low‑volume production use and you accept the tradeoffs of HTML scraping rather than a paid API.

## Remarks
This class issues POST queries to DuckDuckGo's rich HTML endpoint first and, if parsing yields no results, retries against the lite endpoint to work around occasional anomaly/anti-bot responses. It warms a session (so the HttpClientHandler cookie container is populated) and pins a real‑browser User‑Agent for the lifetime of that session to reduce bot detection; a SemaphoreSlim prevents concurrent warmups. Parsing is intentionally forgiving and regex‑driven: parse failures yield an empty result set rather than throwing, and anomaly detections are logged with diagnostic detail so callers can inspect why a query returned no usable results. For high‑volume or production‑grade search, a paid/search API (with explicit SLA and keys) is recommended instead of HTML scraping.

## Notes
- HTML parsing is brittle by nature: changes in DuckDuckGo markup can cause zero results; the implementation treats parse failures as empty results rather than exceptions.
- DuckDuckGo may return an "anomaly" or bot‑challenge page; the class detects and logs that condition and resets session state (including choosing a new User‑Agent) to recover, but repeated automated traffic may still be rate‑limited.
- The named HttpClient must be configured with a CookieContainer (see the project's DependencyInjection helpers). Use the DuckDuckGoWebSearch.HttpClientName named client from IHttpClientFactory so requests hit the intended handlers and share cookie state.

---

## DuckDuckGoWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** constructor

Creates a new DuckDuckGoWebSearch and stores the provided IHttpClientFactory and `ILogger<DuckDuckGoWebSearch>` for use by the instance. Reach for this constructor when registering or resolving the search tool from dependency injection so it can create HTTP clients and emit diagnostic logs.

## Remarks
This constructor centralizes two runtime dependencies: an IHttpClientFactory for obtaining HttpClient instances (avoiding socket exhaustion and managing lifetimes) and an `ILogger<T>` for structured logging from this class. It exists so the class can be composed by DI containers (ASP.NET Core, Generic Host) and keep HTTP client creation and logging concerns externalized.

## Example
```csharp
// In Program.cs / Startup.cs (ASP.NET Core)
services.AddHttpClient();
services.AddTransient<DuckDuckGoWebSearch>();

// Consumption via constructor injection
public class SearchService
{
    private readonly DuckDuckGoWebSearch _search;

    public SearchService(DuckDuckGoWebSearch search)
    {
        _search = search;
    }
}
```

## Notes
- The constructor does not validate its arguments for null; when constructing manually ensure both parameters are non-null (DI containers normally supply non-null instances).
- Using IHttpClientFactory is intentional: it lets the class obtain properly managed HttpClient instances rather than creating them directly.

---

## BuildRequest

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Creates an HttpRequestMessage pre-populated with the set of headers that a modern browser (Chrome/Firefox) would send during a navigation. Use this when performing HTTP requests to DuckDuckGo within this component so the remote server sees a realistic browser navigation (and so session-pinned User-Agent, Sec-Fetch-* and Referer behavior can vary correctly between "initial" and in-site navigations).

## Remarks
This method builds headers per-request rather than modifying HttpClient.DefaultRequestHeaders so the User-Agent can be pinned to the current session (and rotated on session reset) and so the Sec-Fetch-Site/Referer values can differ between an initial warm-up navigation and subsequent same-site navigations. It encodes the browser signals servers commonly use to distinguish a typed/bookmarked entry (no Referer, Sec-Fetch-Site: none) from an in-site navigation (Referer set to the homepage, Sec-Fetch-Site: same-site).

## Example
```csharp
// Initial navigation (no Referer)
var req = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/", isInitialNavigation: true);

// Subsequent navigation from the homepage (includes Referer and same-site fetch)
var req2 = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/?q=example", isInitialNavigation: false);
```

## Notes
- Some runtimes/frameworks restrict certain headers (notably User-Agent and Referer) or provide dedicated properties; adding them with Headers.Add(...) can throw. Verify the target .NET runtime allows these header additions or use the dedicated header properties where appropriate. 
- The method falls back to UserAgents[0] when _sessionUserAgent is null; EnsureSessionAsync is expected to populate the session User-Agent before this method is used.
- isInitialNavigation controls the Referer and Sec-Fetch-Site values only — other header values are identical for both paths.

---

## CleanText

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Decodes HTML entities, removes HTML tags, and trims surrounding whitespace from a piece of text — use this helper when converting HTML-encoded snippets or fragments (for example, search result snippets) into plain text for display or indexing.

## Remarks
This small utility normalizes incoming HTML-like text by first running HtmlDecode (so entities such as &amp; or &lt; become their character equivalents) and then removing any markup with TagStripRegex before trimming. It’s intended as a lightweight transformation for presentation or simple text-processing; it is not a substitute for full HTML sanitization when executing or embedding untrusted markup.

## Example
```csharp
var raw = "&lt;b&gt;Hello&lt;/b&gt; &amp; world  ";
var clean = CleanText(raw); // "Hello & world"
```

## Notes
- Passing null for raw will propagate into the underlying calls and typically results in an ArgumentNullException; callers should ensure the input is non-null or handle null before calling.
- The order matters: decode first so entities inside tags are interpreted, then strip tags; reversing those steps can leave decoded markup behind.
- This method produces plain text and does not perform security-focused sanitization (e.g., it does not guarantee safety against XSS when re-inserting content into HTML).

---

## DetectAnomalyPage

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Detects whether an HTML response is actually an intermediary bot‑block / CAPTCHA / mitigation page (for example those served by DuckDuckGo or Cloudflare) rather than a normal search/results page. Use this after fetching HTML to decide whether to treat the response as a blocked/interstitial page and take a fallback action (retry, use a different endpoint, surface a clear diagnostic) instead of processing it as a valid result.

## Remarks
This is a lightweight heuristic that looks for known markers commonly present in bot‑challenge or mitigation pages. It checks for several strings that have been observed in DuckDuckGo and Cloudflare interstitials (e.g. anomaly markers and Cloudflare challenge tokens). Because providers can change the exact strings or HTML structure, callers should treat a positive result as an indicator to fall back rather than a definitive categorization.

## Example
```csharp
// After downloading HTML for a search, detect if it was blocked and fall back.
string html = await httpClient.GetStringAsync(url);
if (DetectAnomalyPage(html))
{
    // Switch to a fallback endpoint or return a clear error to the caller.
    return HandleBlockedResponse();
}

// Proceed with normal parsing of search results.
ParseSearchResults(html);
```

## Notes
- The method performs case-sensitive checks using StringComparison.Ordinal; markers must match exactly as coded (no culture or case folding).
- html must be non-null: calling this with null will throw a NullReferenceException. Validate or ensure the fetched response is non-null before calling.
- This is a heuristic and can yield false positives or negatives; update the marker list if new mitigation pages are encountered.

---

## EnsureSessionAsync

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Performs a one-time "warm up" navigation to the DuckDuckGo homepage to collect session cookies and commit a stable user-agent for the current session. Call this before issuing search requests when you want the client to mimic a real browser navigation (homepage visit → cookies → query) and reduce DDG heuristics that flag direct query requests.

## Remarks
This method ensures only a single warm-up runs per session: it picks a user-agent at random from the configured UserAgents list and stores it for the session, then issues an HTTP GET to the configured Homepage (marked as an initial navigation) so the HttpClient's CookieContainer can collect cookies set by DuckDuckGo. A Semaphore-like lock prevents concurrent warm-ups; errors during the warm-up are logged but treated as non-fatal so the caller can continue without session cookies.

## Notes
- OperationCanceledException is not swallowed — cancellation will propagate to the caller; all other exceptions are caught, logged, and ignored.
- If the warm-up fails, the method still marks the session as warmed; it will not retry warm-up later in the same session.
- The HttpClient used must have a CookieContainer (or equivalent) configured for cookies to be captured by this request.

---

## FetchAsync

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Performs a DuckDuckGo search GET and returns the response HTML as a string. It intentionally delays a small randomized amount before sending the request and uses a GET with a query string to mimic a real user navigation and reduce bot-detection signals.

## Remarks
This method is an internal helper that builds a GET request for a DuckDuckGo HTML endpoint (it appends q=<escaped query>&kl=us-en), sends it with the provided HttpClient, and returns the raw HTML content. A randomized pre-request delay (200–1200 ms) and using GET instead of POST help the request resemble a normal browser navigation; BuildRequest is used to construct the outgoing HttpRequestMessage (including headers) with isInitialNavigation set to false. On non-success HTTP status codes the method logs a warning and throws an HttpRequestException.

## Example
```csharp
// inside an async method
var html = await FetchAsync(httpClient, "https://html.duckduckgo.com/html", "example search", cancellationToken);
// html contains the returned HTML for the search results page
```

## Notes
- The method waits a randomized 200–1200 ms before sending the request; this is intentional and contributes to overall latency.
- Non-success HTTP responses cause a log entry and an HttpRequestException to be thrown; callers should handle retries or backoff as appropriate.
- The provided CancellationToken is honored for the delay, the HTTP send, and reading the response content; the method returns the raw HTML string and does not perform any parsing.

---

## FirstChars

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Returns the first n characters of the input string s; if s is shorter than or equal to n the original string is returned. Use this helper when you need a concise, in-place truncation without throwing when n is greater than the string length.

## Remarks
This small utility uses C# range slicing (s[..n]) to produce the prefix when the string is longer than n. It centralizes the typical "truncate-but-not-throw" pattern so callers don't need to repeatedly check lengths before taking a substring.

## Example
```csharp
// Typical usage (internal helper):
var text = "Hello, world!";
var first5 = FirstChars(text, 5); // "Hello"

// Safe wrapper to protect against null and unintended negative n:
string SafeFirstChars(string s, int n)
{
    if (s == null) return string.Empty;        // or handle null as appropriate
    if (n <= 0) return string.Empty;           // decide desired behavior for non-positive n
    return FirstChars(s, n);
}
```

## Notes
- The method does not check for null; passing a null s will throw a NullReferenceException.
- Negative n is not explicitly guarded against. Because it uses the range operator, a negative n will be interpreted as an index-from-end (e.g. s[..-1] excludes the last character) — validate n >= 0 if that is not desired.
- If n is greater than or equal to s.Length, the original string instance is returned (no allocation for a shorter copy in that branch).

---

## ParseHtmlEndpoint

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Parses the raw HTML returned by DuckDuckGo and extracts up to the given number of search results as WebSearchResult instances. Use this when you need a lightweight HTML-based extractor for result title, URL and snippet instead of an API-backed response.

## Remarks
This method scans the supplied HTML with precompiled regular expressions: it finds result blocks, extracts a title link and href, decodes HTML entities, normalizes the text (via CleanText) and unwraps redirect URLs before collecting results. It stops once it has collected the requested limit and returns a `List<WebSearchResult>` that may contain fewer items if the page doesn't include enough valid entries. The implementation assumes specific named capture groups (e.g. "body", "href", "text") exist in the regexes it uses and that helper functions like UnwrapRedirect and CleanText perform text normalization.

## Example
```csharp
// inside the same class or a test within the assembly
string html = FetchDuckDuckGoHtml(query); // hypothetical helper
int maxResults = 5;
var parsed = ParseHtmlEndpoint(html, maxResults);
foreach (var r in parsed)
{
    Console.WriteLine($"{r.Title} -> {r.Url}\n{r.Snippet}");
}
```

## Notes
- html must not be null; calling this with null will throw when regexes are run against it.  
- limit must be non-negative; it's used as the initial List capacity and a negative value will throw when creating the list.  
- The method relies on the HTML structure matching the configured regular expressions — changes to DuckDuckGo's markup can break extraction.  
- URLs are passed through UnwrapRedirect and titles/snippets are cleaned; the returned Url may be modified from the original href and Snippet can be an empty string if none is found.

---

## ParseLiteEndpoint

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Parses the HTML returned by DuckDuckGo's "lite" search endpoint and converts it into a list of WebSearchResult objects, up to the provided limit. It decodes HTML entities, cleans title and snippet text, unwraps redirect URLs, and skips any entries that lack a usable title or URL. Use this when you have the raw lite-endpoint HTML and need structured search results for further processing.

## Remarks
The lite layout pairs result links and snippets by document order; this method relies on two precompiled regexes (LiteResultLinkRegex and LiteSnippetRegex) to extract those items and assumes the same ordering. Because a result may be missing a snippet, the snippet index can drift relative to links — the implementation prefers to keep URL/title correctness and accepts that snippets may be absent or occasionally paired incorrectly. The method is side-effect free and returns at most the requested number of results, but may return fewer if matches are missing or filtered out.

## Example
```csharp
// html is the raw response body from DuckDuckGo's lite endpoint
var results = ParseLiteEndpoint(html, 10);
foreach (var r in results)
{
    Console.WriteLine($"Title: {r.Title}");
    Console.WriteLine($"Url: {r.Url}");
    Console.WriteLine($"Snippet: {r.Snippet}\n");
}
```

## Notes
- If a matched link's title or unwrapped URL is empty or whitespace, that entry is skipped, so the returned list can be smaller than the requested limit.
- Snippets and links are matched by index; missing snippet entries can cause subsequent snippets to be misaligned with their links.
- The method expects the regex groups named "href" and "text" to be present in the LiteResultLinkRegex and LiteSnippetRegex matches.
- Raw href values are HTML-decoded and then passed to UnwrapRedirect to obtain the final URL used in the result.

---

## ResetSession

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Marks the current web session as not warmed and clears the chosen user‑agent so that the next search will rebuild the session with a freshly selected UA and perform a homepage round‑trip. Call this when DuckDuckGo returns an anomaly page or any condition that should force a session and fingerprint refresh.

## Remarks
This method deliberately performs a minimal reset: it flips the internal "warmed" flag and nulls the cached user‑agent selection so downstream SearchAsync logic will recreate the session, rotate the UA, and refresh cookies on the next operation. It exists to avoid immediately tearing down or reconstructing session state (which could be more expensive or have ordering implications) and instead defers the rebuild to the natural search flow.

## Example
```csharp
// inside the same class when handling an unexpected DDG response
if (response.IsAnomalyPage) {
    ResetSession(); // ensure next SearchAsync rebuilds the session and rotates UA
    // optionally log or throttle before retrying
}
```

## Notes
- ResetSession does not clear cookies, authentication tokens, or other stored session data beyond the warmed flag and the cached UA — the actual cookie refresh happens when the session is rebuilt during the next SearchAsync. 
- The method is not synchronized; callers should ensure appropriate concurrency control if ResetSession may race with SearchAsync or other session-mutating operations. 
- Calling ResetSession does not immediately perform network activity — it only causes the next search operation to perform the rebuild/round‑trip.

---

## SearchAsync

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Searches DuckDuckGo for the provided query and returns up to the requested number of results. It first attempts the richer html/ endpoint (which yields snippets and redirect-wrapped URLs) and, if that fails or returns no parseable results, falls back to the simpler lite/ endpoint. Use this method when you need programmatic web search results from DuckDuckGo and want built-in handling for endpoint fallback and bot-detection recovery.

## Remarks
This method encapsulates a two-stage retrieval strategy: primary retrieval from the html/ endpoint (preferred for richer results) and a fallback to the lite/ endpoint when the primary fails to return usable results or appears to be an anomaly/block page. If either endpoint responds with an anomaly page, the method logs a warning; if both endpoints indicate being blocked it resets the local session state and throws an InvalidOperationException with guidance about provider-based alternatives. If both endpoints return valid HTML but no parseable results, the method returns an empty list rather than throwing.

## Example
```csharp
var results = await duckDuckGoWebSearch.SearchAsync("how to bake sourdough", 10, cancellationToken);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}");
}
```

## Notes
- If both html/ and lite/ endpoints return an anomaly/CAPTCHA page, the method calls ResetSession() and throws InvalidOperationException; the exception message recommends using paid provider integrations for reliable scraping.
- The method forwards the provided CancellationToken to its async operations; callers can cancel the request by cancelling that token.
- A successful call may still return an empty `IReadOnlyList<WebSearchResult>` when the endpoints return 200 with no parseable results for the query.
- The method logs informational and warning messages at several decision points (zero results, anomaly detection); these logs can help diagnose blocking or parsing issues.

---

## UnwrapRedirect

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

Decodes DuckDuckGo's redirect wrapper URLs of the form "/l/?uddg=ENCODED_URL&..." and returns the underlying destination. If the input doesn't match that wrapper shape the method falls back to returning the original href; it also converts protocol-relative URLs (starting with "//") to an absolute https: URL.

## Remarks
This is a small, defensive utility used where search-result links need to be resolved to their real targets instead of DuckDuckGo's link-wrapping redirect. It intentionally only looks for the exact marker (`uddg=`) and attempts a single URL-unescape of the parameter; any decoding error is swallowed and the original href is returned so callers can continue processing without throwing.

## Example
```csharp
var wrapped = "/l/?uddg=https%3A%2F%2Fexample.com%2Fpath%3Fq%3D1&rut=someValue";
var target = UnwrapRedirect(wrapped);
// target == "https://example.com/path?q=1"

var protocolRelative = "//example.com/path";
var absolute = UnwrapRedirect(protocolRelative);
// absolute == "https://example.com/path"
```

## Notes
- The marker lookup is case-sensitive (uses StringComparison.Ordinal); variants like "UDDG=" will not be recognized.
- If decoding fails (malformed percent-encoding, etc.) the original href is returned — the method does not throw.
- The method does not validate that the decoded string is an absolute or safe URL; callers should validate or normalize the result if needed.

---

## Homepage

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

Holds the DuckDuckGo homepage URL used as the base URI when building HTTP requests inside DuckDuckGoWebSearch. Reach for this constant when updating the target host or changing how search requests are constructed; it is private and intended for internal use only.

## Remarks
Centralizes the root host for all requests made by DuckDuckGoWebSearch so the class constructs URLs from a single canonical source. Being a const keeps the value fixed at compile time and avoids scattering the literal throughout the implementation; if the endpoint needs to vary by environment or at runtime, prefer a configurable value instead.

## Example
```csharp
// Combine safely to avoid double slashes and to preserve query formatting
var baseUri = new Uri(Homepage);
var searchUri = new Uri(baseUri, "?q=dotnet+docs");
```

## Notes
- The value includes a trailing slash; naive string concatenation can produce duplicate slashes — prefer Uri-based combination as shown above.
- It's a compile-time constant: changing the value requires recompiling the containing assembly.
- If the service endpoint may change per environment or at runtime, move this into configuration rather than a const.

---

## HtmlEndpoint

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

Absolute URL for DuckDuckGo's primary HTML search endpoint. This constant is used when the code needs to send requests to the rich HTML-rendered search page (https://html.duckduckgo.com/html/) and is intentionally an absolute URI so requests always target the correct host regardless of any HttpClient BaseAddress configuration.

## Remarks
The project supports two DuckDuckGo search endpoints (a rich HTML output and a minimal "lite" output). Using an absolute URL here prevents accidental requests from being sent to the wrong subdomain when a named HttpClient has a BaseAddress set — a previous implementation that used the wrong host caused the lite fallback to silently fail because html.duckduckgo.com does not serve the lite layout. The trailing slash is included to simplify path concatenation when building query requests.

## Notes
- The value is an absolute URI and will override a configured HttpClient BaseAddress; do not assume BaseAddress will be applied.
- The response from this endpoint is HTML, not JSON — callers should parse HTML accordingly.
- This constant targets the rich HTML endpoint only; the lite (fallback) endpoint is a separate URL and must be used explicitly when needed.

---

## HtmlResultBlockRegex

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

Matches the HTML blocks representing individual search results in DuckDuckGo's server-rendered result pages and captures the block's inner HTML into the named group "body".

This regex is used when parsing the full HTML search results page to extract each <div> whose class contains the token "result"; it consumes up to the next result <div>, the element with id "bottom_spacing", or the end of input. The pattern is compiled and uses Singleline mode so the dot matches newlines, allowing the capture to include multiline HTML.

## Remarks
This field centralizes the HTML block extraction logic for the DuckDuckGo web-search parsing path so higher-level code can operate on each result's inner HTML (the captured "body") rather than scanning the raw page. The comment in the source notes that the server-generated structure is expected to be stable; if DuckDuckGo changes the markup the surrounding code falls back to returning zero results and may try an alternate "lite/" endpoint.

## Notes
- The regex is not a full HTML parser: if the page structure changes (different class names, nesting, or layout), matches may fail or produce incomplete captures.
- The pattern uses RegexOptions.Compiled (one-time compile cost at startup) and RegexOptions.Singleline (dot matches newline). The compiled instance is reused and is thread-safe for concurrent use.
- The capture named "body" contains raw inner HTML for the result block; callers should treat it as untrusted input and sanitize or parse it before rendering or extracting structured fields.

---

## HtmlSnippetRegex

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

Matches an anchor (<a>) element whose class attribute contains the token "result__snippet" and captures the anchor's inner HTML/text into a named group "text". Use this when scraping DuckDuckGo search result pages to quickly extract the displayed snippet from result links without bringing a full HTML parser into the code path.

## Remarks
This regex is a lightweight, performance-oriented extractor intended for internal scraping of DuckDuckGo result HTML. It looks specifically for a class attribute that contains the word "result__snippet" (respecting word boundaries), allows other attributes on the same tag, and captures everything between the tag's opening and closing <a> into the "text" group. RegexOptions.Singleline makes "." match newlines so the capture can span multiple lines, and RegexOptions.Compiled improves runtime performance for repeated matches.

## Example
```csharp
var match = HtmlSnippetRegex.Match(html);
if (match.Success)
{
    // raw inner HTML/text of the matched <a>
    var snippetHtml = match.Groups["text"].Value;
    // you may want to decode HTML entities or strip tags after this
}
```

## Notes
- The pattern assumes double-quoted attribute values; it will not match if the class attribute uses single quotes.
- This is a brittle HTML scraping approach: reordered attributes, nested anchors, or unexpected markup can break the match. Prefer an HTML parser (e.g., HtmlAgilityPack) for robust extraction.
- The captured group may contain nested tags or HTML entities; post-processing (strip tags / HTML decode) is often required to obtain plain text.
- Compiled regexes have a startup cost but are beneficial for repeated use; the static readonly field avoids recompilation per-call.


---

## HtmlTitleLinkRegex

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

A compiled regular expression used to locate DuckDuckGo result anchor elements (the title links) in an HTML result page. It captures the link target in the named group "href" and the anchor inner HTML in the named group "text". Reach for this field when parsing DuckDuckGo search result HTML within the DuckDuckGoWebSearch implementation instead of re-creating the pattern each time.

## Remarks
This field is declared static and readonly to reuse a single compiled Regex instance for performance across calls. The pattern looks for <a> tags whose class attribute contains the token result__a, then extracts the href attribute and the element's contents. RegexOptions.Singleline is used so the dot (.) also matches newlines, and RegexOptions.Compiled improves match throughput at the cost of one-time JIT/compile overhead.

## Example
```csharp
// Iterate all result title links from an HTML response
var matches = HtmlTitleLinkRegex.Matches(htmlResponse);
foreach (Match m in matches)
{
    var href = m.Groups["href"].Value;   // the URL from the href attribute
    var innerHtml = m.Groups["text"].Value; // the anchor's inner HTML (may contain tags)
    // If plain text is required, strip tags or use an HTML parser
}
```

## Notes
- This regex is brittle as an HTML parser substitute: it expects double-quoted attributes and may fail on single quotes or unusual attribute ordering/spacing.
- The pattern matches based on a class token (\bresult__a\b); if DuckDuckGo changes class names or markup, the regex will stop matching.
- RegexOptions.Compiled improves runtime performance but has a one-time compilation cost and slightly higher memory use; the static readonly lifetime makes it safe and efficient for repeated use.

---

## HttpClientName

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

Constant holding the DI name for the HttpClient used to perform DuckDuckGo web searches. Use this constant when registering or resolving the named HttpClient to avoid scattering the literal string "DuckDuckGoSearch" across the codebase.

## Remarks
Centralizes the HttpClient registration name so callers and registration code stay consistent and avoid typos. Because this is a public compile-time constant, consumers reference the name directly rather than an instance of an HttpClient; the constant itself does not create or configure any network client.

## Example
```csharp
// Register the named HttpClient during startup/configuration
services.AddHttpClient(HttpClientName, client =>
{
    client.BaseAddress = new Uri("https://duckduckgo.com/");
    // configure default headers, timeouts, etc.
});

// Later, resolve the named client via IHttpClientFactory
var client = httpClientFactory.CreateClient(HttpClientName);
// use client to perform searches
```

## Notes
- Because this is a public const, its value is inlined into referencing assemblies at compile time; changing the string requires recompiling all dependent assemblies to observe the new value.
- This constant is only a name — it does not configure or instantiate an HttpClient by itself. Ensure the named client is registered in DI before attempting to resolve it.

---

## LiteEndpoint

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

Constant containing the DuckDuckGo "Lite" search endpoint URL used by this class to build HTTP requests against DuckDuckGo's lightweight HTML search interface. Reach for this when composing internal requests that should target the compact "lite" page rather than a full web-search API or UI.

## Remarks
Centralizes the endpoint so the URL is defined in one place and easy to update if the service changes. The constant includes a trailing slash to simplify concatenation with query strings or path segments; callers within the class should avoid duplicating slashes when combining values.

## Example
```csharp
// inside DuckDuckGoWebSearch
var uri = LiteEndpoint + "?q=" + Uri.EscapeDataString(query);
var html = await httpClient.GetStringAsync(uri);
```

## Notes
- The trailing slash is intentional; when appending paths do not add a leading slash or you'll create a double-slash in the URL.
- This value targets DuckDuckGo's lightweight HTML interface (not an official API) and may change if the provider updates their endpoints.
- The field is private and intended for internal use only; external callers should not depend on its exact value.

---

## LiteResultLinkRegex

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

Matches anchor elements produced by DuckDuckGo's "lite" HTML endpoint and extracts the link target and displayed text. Use this when parsing results returned from the lite endpoint (which uses single-quoted class attributes and places the result link and snippet on adjacent rows) to quickly pull the href value and inner text without a full HTML parser.

## Remarks
This precompiled, Singleline regex is tuned to the lite endpoint's specific HTML shape: it expects an href attribute with double quotes followed later by a class='result-link' attribute, then captures the anchor's inner content into a named group. Being a static, compiled Regex improves performance when scanning many results and ensures thread-safe reuse.

## Notes
- The pattern assumes the href attribute appears before the class='result-link' attribute; anchors with a different attribute order will not match.
- It is fragile to HTML variations: it requires double quotes for href and single quotes for the class attribute (as emitted by the lite endpoint), so different quoting or markup changes will break matches.
- This extracts the anchor's inner HTML (the text group may include markup); it does not validate or normalize the captured URL and is not a substitute for an HTML parser when robust parsing is required.

---

## LiteSnippetRegex

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

Matches the inner HTML of a DuckDuckGo search-result snippet cell. Use this when extracting the contents of a <td class='result-snippet'> element from raw HTML returned by a DuckDuckGo results page; the named capture group "text" contains the snippet's inner HTML.

## Remarks
This compiled Regex is optimized for repeated use when parsing search result pages: it runs with RegexOptions.Singleline so the dot (.) spans newlines and RegexOptions.Compiled for better performance on multiple matches. The pattern requires a td element whose class attribute is exactly 'result-snippet' (single quotes) and captures everything between the opening and closing td tags into the "text" group.

## Example
```csharp
string html = /* HTML of a results page or fragment */;
var match = LiteSnippetRegex.Match(html);
if (match.Success)
{
    // inner HTML of the <td class='result-snippet'>
    string snippetHtml = match.Groups["text"].Value;
    // further processing: HTML decoding or stripping tags as needed
}
```

## Notes
- The pattern looks for class='result-snippet' using single quotes and is case-sensitive; it will not match class="result-snippet" or different casing.
- The capture returns the raw inner HTML (may include child tags); callers must decode/strip HTML if plain text is required.
- Using a regex to parse HTML is brittle: changes in DuckDuckGo's markup, attribute ordering, or additional attributes may cause misses; consider an HTML parser for robust extraction.

---

## TagStripRegex

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

A compiled regular expression that matches simple HTML/XML-style tags (any sequence starting with '<' and ending with '>'). Used to remove or replace tag-like substrings from text when converting small snippets of HTML to plain text; chosen as a single, reusable static instance to avoid recompiling the pattern on each use.

## Remarks
This is a pragmatic, lightweight pattern for stripping tag-like constructs and is provided as a private static readonly to centralize the pattern and improve performance through RegexOptions.Compiled. It is intended for simple sanitization/cleanup scenarios inside the DuckDuckGoWebSearch implementation rather than full HTML parsing.

## Example
```csharp
// Typical usage inside the containing class
string html = "<p>Hello <b>World</b></p>";
string plain = TagStripRegex.Replace(html, ""); // yields "Hello World"
```

## Notes
- This regex is not an HTML parser: it can be tripped by malformed HTML, tags containing '>' inside quoted attributes, comments, CDATA, or scripts/styles that include angle brackets.
- Compiled Regex instances carry a small startup/compile cost but perform better when reused frequently; hence the static readonly choice.
- System.Text.RegularExpressions.Regex instances are safe for concurrent use, so this static field can be used from multiple threads without additional synchronization.


---

## UserAgents

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

A small, fixed pool of recent real-browser User-Agent strings used by the web search tool to present a plausible browser identity. The intent is to select a single User-Agent at session warmup and hold it for the life of the session (or until anomaly detection resets it), rather than rotating the UA on every request.

## Remarks
This pool exists to reduce bot-like behavior while avoiding rapid per-request UA changes, which themselves are a strong bot signal. Keeping the list small and realistic spreads fingerprints across deployments and restarts without introducing excessive variability. The field is declared static readonly so the reference is stable for the process lifetime; it is not intended to be modified at runtime.

## Example
```csharp
// Choose one UA at session startup and reuse for the session
var rng = new Random();
string sessionUserAgent = UserAgents[rng.Next(UserAgents.Length)];
// then apply sessionUserAgent to outgoing HTTP requests' User-Agent header
```

## Notes
- The readonly modifier prevents reassignment of the array reference but does not make the array's contents immutable; do not mutate the elements at runtime.
- Rotating the User-Agent per request is intentionally avoided because it increases detectability; prefer one UA per session unless you have a robust session-reset strategy.
- The list is a snapshot of common desktop browser UAs — update it periodically to match current browser releases and to add other platforms if needed.


---
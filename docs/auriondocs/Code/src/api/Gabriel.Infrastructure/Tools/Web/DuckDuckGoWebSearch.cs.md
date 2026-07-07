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

```csharp
public sealed class DuckDuckGoWebSearch : IWebSearch
```


A zero-configuration IWebSearch implementation that scrapes DuckDuckGo's public HTML search endpoints (html/ first, then lite/ as a fallback) so callers can perform web searches without an API key. Reach for this when you need an out‑of‑the‑box, keyless search backend for development, lightweight deployments, or scenarios where a paid search API is not available; for production-scale, high-volume, or strongly reliable search results prefer a paid API.

## Remarks
This class wraps two DuckDuckGo HTML endpoints and handles the practical workarounds needed when scraping a public site: it pre-warms a session (visiting the homepage to populate the HttpClientHandler's CookieContainer), picks and sticks to a realistic User‑Agent from a small pool for the duration of the session, and uses a SemaphoreSlim to dedupe concurrent warmups. The implementation prefers the richer html/ output and falls back to lite/ if parsing returns no results; parsing is intentionally forgiving and regex-driven so failures produce empty result sets instead of exceptions. Anomaly or rate‑limit pages are detected and logged with diagnostic detail to aid debugging. Absolute endpoint URLs are used so requests always hit the intended DuckDuckGo hosts regardless of any configured HttpClient BaseAddress.

## Notes
- Parsing failures (HTML shape changes or unexpected responses) yield zero results rather than throwing — check logs for diagnostic details if you see unexpected empty responses.
- This approach has tradeoffs vs. a paid API: no key/quota makes it convenient, but HTML can change and DuckDuckGo may rate‑limit or serve anomaly/CAPTCHA pages under heavy traffic.
- A single User‑Agent is chosen at session warmup and held for the session; the class does not rotate User‑Agent per request (rapid UA changes in a session are avoided because they look like bot behavior).

---

## DuckDuckGoWebSearch
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** constructor

```csharp
public DuckDuckGoWebSearch(IHttpClientFactory httpFactory, ILogger<DuckDuckGoWebSearch> logger)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `httpFactory` | `IHttpClientFactory` | — |
| `logger` | `ILogger<DuckDuckGoWebSearch>` | — |

**Returns:** `public`


Initializes a new instance of the `DuckDuckGoWebSearch` class with the specified HTTP client factory and logger. This constructor is used to set up the necessary dependencies for performing web searches via DuckDuckGo, enabling HTTP requests and logging capabilities within the search implementation.

---

## BuildRequest
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private HttpRequestMessage BuildRequest(HttpMethod method, string url, bool isInitialNavigation)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `method` | `HttpMethod` | — |
| `url` | `string` | — |
| `isInitialNavigation` | `bool` | — |

**Returns:** `HttpRequestMessage`


BuildRequest constructs an HttpRequestMessage populated with a full set of headers that mimic a real browser navigation, enabling session-specific user agent handling and correct Sec-Fetch headers for initial vs in-site navigation. It builds the request per call rather than reusing HttpClient.DefaultRequestHeaders to keep UA rotation and site-context accurate across navigations.

## Remarks
Centralizes the per-request header construction to decouple that concern from HttpClient defaults and to enable precise control over the request's identity. The method selects a session-bound User-Agent (falling back to a default if none is available) and applies the proper Sec-Fetch-* headers depending on whether this is the initial navigation (no referrer, Sec-Fetch-Site: none) or an in-site navigation (same-site with a Referer).

## Notes
- _sessionUserAgent fallback behavior: if null, uses UserAgents[0] as a fallback.
- This method handles header-level browser emulation only; cookies and credentials are not set here.
- Be mindful of header conflicts if headers are overridden later by HttpClient or middleware.


---

## CleanText
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private static string CleanText(string raw)
        => TagStripRegex.Replace(WebUtility.HtmlDecode(raw), "").Trim()
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `raw` | `string` | — |

**Returns:** `string`


Converts a raw HTML-laden string into clean, plain text by decoding HTML entities, stripping markup, and trimming whitespace. CleanText uses WebUtility.HtmlDecode to convert encoded characters (for example, &amp; becomes &) and then applies TagStripRegex to remove HTML tags, returning a trimmed result suitable for display or logging. This helper is intended for internal use where text sourced from external content (such as web search results) must be sanitized before presentation.

## Remarks
By centralizing this sequence (decode-then-strip-trim) in a single private helper, the codebase ensures consistent text normalization across all consumers of the DuckDuckGoWebSearch results. It also encapsulates the exact tag-removal strategy behind TagStripRegex, so changes to how tags are stripped affect all call sites in one place.

## Notes
- Input must be non-null; no explicit null check exists in the method. Ensure upstream callers pass a non-null string.
- TagStripRegex's coverage determines what HTML tags are removed; if the regex doesn't cover certain constructs, some markup may remain in the output.

---

## DetectAnomalyPage
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private static bool DetectAnomalyPage(string html)
        => html.Contains("anomaly_modal", StringComparison.Ordinal)
        || html.Contains("anomaly-modal", StringComparison.Ordinal)
        || html.Contains("anomaly.js", StringComparison.Ordinal)
        
        || html.Contains("Just a moment", StringComparison.Ordinal)
        || html.Contains("cf-mitigated", StringComparison.Ordinal)
        || html.Contains("cf-browser-verification", StringComparison.Ordinal)
        || html.Contains("__cf_chl_", StringComparison.Ordinal)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `html` | `string` | — |

**Returns:** `bool`


DetectAnomalyPage inspects a block of HTML and returns true when the content appears to be an anti-bot or Cloudflare anomaly page. It looks for a handful of well-known markers (anomaly_modal, anomaly-modal, anomaly.js, the Cloudflare interstitial phrase 'Just a moment', and related indicators like cf-mitigated, cf-browser-verification, or __cf_chl_) and treats any match as an anomaly. Callers use the result to decide whether to fall back to an alternative endpoint or surface a clearer diagnostic instead of misclassifying the response as a genuine empty result.

## Remarks
Centralizes anomaly-page detection for DuckDuckGo's web surface. By aggregating multiple markers in one helper, it decouples the caller from specific page variants and makes it easier to adapt when Cloudflare or similar blocks evolve. It’s designed to be fast (ordinal string comparisons) and private to enforce a single sourcing of this heuristic.

## Notes
- Null input will throw; ensure the html argument is non-null before calling.
- The marker set is tied to known Cloudflare/anti-bot phrases; if the service changes its blocks, this detection may miss new variants unless updated.

---

## EnsureSessionAsync
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private async Task EnsureSessionAsync(HttpClient http, CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `http` | `HttpClient` | — |
| `ct` | `CancellationToken` | — |

**Returns:** `Task`


This asynchronous method performs a one-time initialization of a DuckDuckGo search session by sending a GET request to the DuckDuckGo homepage. It ensures that session cookies, which are necessary for subsequent search queries to avoid heuristic blocking, are obtained and stored in the HTTP client's cookie container. The method also selects and commits to a single user agent string for the session to mimic real browser behavior and avoid detection as scripted traffic.

The session warming is guarded by a semaphore to prevent concurrent initializations and is only performed once per instance. If the homepage request fails (except for cancellation), the method logs a warning but allows the search process to continue without the session cookies, making the warm-up non-fatal.

## Remarks

This method addresses DuckDuckGo's heuristic that flags direct query requests without prior navigation to the homepage. By simulating a real browser's initial navigation and cookie acquisition, it helps avoid triggering anti-bot measures and improves the reliability of search requests. The consistent user agent selection further reduces the chance of detection by mimicking typical browser session behavior.

## Notes

- The method uses a semaphore to ensure thread-safe, single execution of the session warm-up.
- Failure to warm the session does not prevent search queries but may reduce their success rate due to missing cookies.
- The user agent is randomly chosen once per session and reused to avoid rapid changes that could be flagged as scripted.

---

## FetchAsync
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private async Task<string> FetchAsync(HttpClient http, string url, string query, CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `http` | `HttpClient` | — |
| `url` | `string` | — |
| `query` | `string` | — |
| `ct` | `CancellationToken` | — |

**Returns:** `Task<string>`


FetchAsync encapsulates the transport for performing a DuckDuckGo search: it waits a small randomized delay, issues a GET request with the encoded query, and returns the response body as a string. If the response is not successful, it logs a warning and throws HttpRequestException to let callers handle the error.

## Remarks
This abstraction centralizes the HTTP fetch logic for the DuckDuckGo flow, shielding callers from request construction, delay strategy, and error handling. It ensures the same query-building pattern (q parameter, locale kl) across all call sites and makes it easier to adapt to changes in the remote endpoint or throttling strategy. Keeping this as a private helper maintains consistency and reduces duplication in the search orchestration code.

## Notes
- No retries or exponential backoff are implemented here; callers needing resilience should implement it at a higher level.
- Cancellation is honored via the provided CancellationToken; canceling will interrupt the delay or the HTTP request.
- The URL composition assumes the base URL does not already contain query parameters; if that changes, this method might produce an invalid URL.

---

## FirstChars
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private static string FirstChars(string s, int n)
        => s.Length <= n ? s : s[..n]
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `s` | `string` | — |
| `n` | `int` | — |

**Returns:** `string`


Returns at most n characters from the start of the input string. If the input string's length is less than or equal to n, the string is returned unchanged; otherwise, the substring consisting of the first n characters is returned. This tiny helper centralizes the common pattern of truncating text for previews, UI fields, or compact logs, so callers can obtain a stable, fixed-length representation without writing conditional length checks themselves.

## Remarks
Encapsulates a common truncation pattern to keep the codebase consistent when a fixed-width display is needed. It reduces boilerplate by factoring the length check and substring logic into a single, private utility that other methods can rely on when constructing short previews or summaries.

## Example
```csharp
string s = "Gabriel";
string t = FirstChars(s, 3); // t == "Gab"
```

## Notes
- Null input will throw NullReferenceException.
- Negative n is not handled and will throw due to the range end being negative.


---

## ParseHtmlEndpoint
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private static List<WebSearchResult> ParseHtmlEndpoint(string html, int limit)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `html` | `string` | — |
| `limit` | `int` | — |

**Returns:** `List<WebSearchResult>`


Parses an HTML string containing search results and returns a list of WebSearchResult objects up to the specified limit. It scans the HTML using HtmlResultBlockRegex to locate individual result blocks, extracts the body, and then uses HtmlTitleLinkRegex to obtain the title and href. The href is HTML-decoded and passed through UnwrapRedirect to resolve a final URL. If both a non-empty title and URL are found, the snippet is optionally extracted with HtmlSnippetRegex and cleaned; the result is added as a WebSearchResult. The process repeats until the limit is reached or no more blocks remain.

## Remarks
This helper centralizes the translation of HTML result blocks into domain objects (WebSearchResult). It encapsulates the brittle HTML parsing logic behind a single, testable boundary, so callers can request a fixed number of results without dealing with the HTML structure. The method relies on a small set of regular expressions defined elsewhere and quietly skips malformed blocks rather than throwing, which keeps consuming code simple.

## Example
```csharp
// Given HTML from a DuckDuckGo-like endpoint, parse up to 5 results
var results = ParseHtmlEndpoint(html, 5);
```

## Notes
- Regex-based HTML parsing is brittle and sensitive to changes in the endpoint's HTML.
- A negative limit will throw due to the List capacity argument when constructing the results collection.
- If limit is 0, the method returns an empty list without processing.


---

## ParseLiteEndpoint
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private static List<WebSearchResult> ParseLiteEndpoint(string html, int limit)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `html` | `string` | — |
| `limit` | `int` | — |

**Returns:** `List<WebSearchResult>`


ParseLiteEndpoint is a private static helper that converts raw HTML from DuckDuckGo's lite search endpoint into a list of WebSearchResult objects. It discovers result links with LiteResultLinkRegex and associated per-link snippets with LiteSnippetRegex, HTML-decodes the href, and unwraps any redirect wrappers to obtain the final URL. The method respects the provided limit, skips entries with missing or whitespace-only titles or URLs, and assigns a snippet to each link when available (otherwise an empty snippet). Snippet matching is index-based: the snippet for a link is taken from the snippet matches at the same position, if present. This function is used by higher-level search orchestration to materialize strongly-typed results from the lite HTML.

---

## ResetSession
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private void ResetSession()
```

**Returns:** `void`


Resets the internal session state used for DuckDuckGo interactions by clearing the warmed flag and the selected user agent. This causes the next SearchAsync to rebuild the session with a freshly picked UA and trigger another homepage round-trip, refreshing the cookie jar and subtly changing the fingerprint.

## Remarks
This method encapsulates the recovery semantics for the DuckDuckGo web flow. By isolating the reset logic, callers can recover from anomaly pages without duplicating state-management details, and the next operation will reinitialize the session environment from a clean slate.

## Example
```csharp
// In anomaly handling, reset internal session so the next request uses a new UA
ResetSession();
```

## Notes
- Private method; not part of the public API and should only be invoked by internal anomaly handling paths.
- Resetting the UA and cookies may change server-side fingerprinting; ensure downstream logic can tolerate a fresh UA on the next request.

---

## SearchAsync
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
public async Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `query` | `string` | — |
| `limit` | `int` | — |
| `ct` | `CancellationToken` | — |

**Returns:** `Task<IReadOnlyList<WebSearchResult>>`


SearchAsync implements a resilient web search operation against DuckDuckGo by attempting a rich html/ endpoint first and falling back to a lighter lite/ endpoint when needed. It returns up to the requested limit of WebSearchResult items and will throw InvalidOperationException if both endpoints present an anomaly or block page, signaling that scraping has been blocked and a different strategy (e.g., a Brave/Tavily API with a key) should be used. If neither endpoint yields results, it returns an empty list.

## Remarks
This method encapsulates a two-tier fetch strategy to maximise reliability in the face of anti-bot protections. It prefers the richer html/ endpoint for better result parsing, but gracefully degrades to the lite/ endpoint when the former is blocked or unparseable. If both endpoints report anomalies, the session is reset to re-warm with a different user-agent and cookies, and an exception is raised to alert callers that a switch to a dedicated API or proxy-based approach is required. Logging around each stage helps diagnose which endpoint was used, whether a fallback occurred, and the outcome of the search.

## Notes
- Throws InvalidOperationException when both html/ and lite/ endpoints return anomaly or block pages; callers should handle this case or switch to an alternate scraping strategy (e.g., Brave/Tavily with a matching API key).
- If neither endpoint yields results, the method returns an empty `IReadOnlyList<WebSearchResult>` rather than null, signaling a genuine no-results outcome.
- The operation may perform up to two HTTP fetches per invocation (one per endpoint) plus session maintenance, so callers should consider cancellation handling and potential latency in tight loops.

---

## UnwrapRedirect
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private static string UnwrapRedirect(string href)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `href` | `string` | — |

**Returns:** `string`


UnwrapRedirect decodes the real navigation target from a DuckDuckGo redirect wrapper. When the href contains the uddg= parameter, the function extracts the encoded URL, decodes it with Uri.UnescapeDataString, and returns the result; if the wrapper is not recognized or decoding fails, it falls back to returning the original href. If the input is a protocol-relative URL (starting with //), it normalizes it to https:// to produce an absolute URL. This small, pure utility centralizes the unwrapping/decoding logic so call sites can reliably obtain canonical targets without duplicating wrapper-specific parsing.

## Remarks
UnwrapRedirect isolates the URL-unwrapping logic so downstream code doesn't need to replicate the same checks wherever a redirect URL might appear. It provides a safe fallback path, returning the input when decoding cannot be performed, and ensures protocol-relative links become absolute for consistent downstream processing.

## Example
```csharp
// Common case: unwrap a DDG redirect
var target = UnwrapRedirect("https://duckduckgo.com/l/?uddg=https%3A%2F%2Fexample.com%2Fpage");
```

## Notes
- Caller must not pass null; the method does not guard against null input and will throw a NullReferenceException if href is null.
- Decoding errors are swallowed and the original href is returned.

---

## Homepage
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string Homepage = "https://duckduckgo.com/"
```


This private constant holds the canonical DuckDuckGo homepage URL used by the internal web search logic. Developers reference this value when assembling request URLs or navigating to the DuckDuckGo homepage to ensure a consistent base URL across the class.

## Remarks

This constant centralizes the external endpoint used by the search workflow, preventing drift and simplifying maintenance. Its private, compile-time nature enforces encapsulation: external callers cannot rely on or override this value, and any change requires a code change and recompilation. In practice, it keeps URL construction predictable when building full request URLs by appending paths or query components.

## Notes

- Because it is inlined at compile time, updating the URL requires recompiling all assemblies that reference this symbol.
- There is no runtime configurability; if you need environment-specific endpoints, consider moving the value to configuration or exposing a public accessor with a configurable backing store.

---

## HtmlEndpoint
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string HtmlEndpoint = "https://html.duckduckgo.com/html/"
```


The HtmlEndpoint field is a private constant that stores the absolute URL to DuckDuckGo's primary HTML search endpoint, guaranteeing that requests target the intended host regardless of HttpClient.BaseAddress configuration. The accompanying comments also reflect the existence of a separate lite endpoint (https://lite.duckduckgo.com/lite/) used for minimal HTML output as a fallback. Older versions pointed both endpoints at html.duckduckgo.com, which silently broke the lite layout.

## Remarks
This constant centralizes the endpoint configuration, making it easy to adjust the target hosts in one place and ensuring the rest of the code consistently uses the correct URLs for both rich HTML and lite fallbacks. By using absolute URIs, the implementation remains resilient to varying HttpClient base addresses and prevents host misrouting.

## Notes
- The field is private and compiled as a constant; its value is baked into the assembly and cannot be changed at runtime.
- If you modify the endpoints, ensure the lite endpoint remains available and the hosts remain correct to avoid breaking minimal HTML delivery.
- The trailing slash in the URL is significant for URL composition; removing or altering it could lead to incorrect paths.

---

## HtmlResultBlockRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlResultBlockRegex = new(
        @"<div[^>]*class=""[^""]*\bresult\b[^""]*""[^>]*>(?<body>.*?)(?=<div[^>]*class=""[^""]*\bresult\b|<div[^>]*id=""bottom_spacing""|\Z)",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


HtmlResultBlockRegex is a private, compiled Regex that extracts the HTML fragment for a single search result from the HTML returned by the search endpoint. It assumes server-generated markup where each result is a div whose class contains "result" and captures the inner body up to the next result block, the bottom-spacing marker, or end of the document.

## Remarks
This primitive serves as a low-level parsing aid that turns the raw HTML into discrete result blocks, enabling higher-level logic to focus on extracting the title, snippet, and URL from each block. By anchoring the extraction to the server’s stable structure, downstream code can operate with simple, repeatable blocks; if the endpoint changes its markup, the parsing can fail gracefully and the caller may fall back to a lighter parsing path as described in the surrounding comments.

## Notes
- Fragility to upstream HTML changes: the pattern relies on a stable server-side structure; structural changes may break matches and may trigger a fallback behavior in callers.
- Boundary correctness: the regex uses a lookahead to stop at the next result block or the bottom spacer; variations in markup around these anchors could yield partial or overrun captures. Ensure input consistently follows the expected structure.


---

## HtmlSnippetRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlSnippetRegex = new(
        @"class=""[^""]*\bresult__snippet\b[^""]*""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


HtmlSnippetRegex is a private static readonly Regex used to extract the visible snippet text from a DuckDuckGo HTML search result item. It matches anchor elements whose class attribute includes the word result__snippet and captures the inner text into a named group called text, letting the caller retrieve the snippet without brittle index-based parsing. Use this when you need to pull the human-readable snippet from the HTML results instead of writing ad-hoc string operations.

## Remarks
By centralizing the snippet extraction into a single, compiled pattern, this field reduces duplication and improves performance when parsing many results. The use of RegexOptions.Singleline ensures the snippet capture spans across line breaks, and RegexOptions.Compiled minimizes execution overhead on repeated matches. Because the field is private, any reuse relies on the class's internal parsing logic, which helps maintain a stable surface for changes to the underlying HTML structure.

## Example
```csharp
// Example usage within the class that defines HtmlSnippetRegex
string html = "<a class=\"result__snippet\" href=\"/link\">Example snippet text</a>";
var m = HtmlSnippetRegex.Match(html);
if (m.Success)
{
    string snippet = m.Groups["text"].Value;
    Console.WriteLine(snippet);
}
```

## Notes
- Fragile against changes in the HTML structure: if the class name or surrounding markup changes, the regex may fail to match.
- The field is private and not directly reusable by external code; reuse is confined to the containing class.
- If the HTML contains multiple matching anchors, decide whether to use Match (first) or enumerate all matches and select the desired one.

---

## HtmlTitleLinkRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlTitleLinkRegex = new(
        @"<a[^>]*class=""[^""]*\bresult__a\b[^""]*""[^>]*href=""(?<href>[^""]+)""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


This private static readonly Regex holds the compiled pattern used to extract the primary result link from a DuckDuckGo search results page. It matches an anchor tag whose class attribute contains the result__a class and captures two pieces of information: the destination URL (href) and the visible link text. The Regex is stored as a static readonly field so it is compiled once and reused across invocations, avoiding repeated compilation and ensuring consistent parsing of DuckDuckGo HTML results.

## Remarks
The abstraction centralizes the brittle HTML-structure dependency of DuckDuckGo's results. Because the pattern relies on specific markup (the result__a class and the anchor tag shape), changes to the page could break parsing; having a single, centralized Regex makes maintenance straightforward. Additionally, the Regex instance is safe for concurrent use across threads since it is static, readonly, and immutable after construction. The named capture groups href and text map to the link URL and its display text, respectively.

## Notes
- The parsing logic is tightly coupled to DuckDuckGo's current HTML structure; any markup/class-name changes require an update to the pattern.
- Callers must reference the named groups "href" and "text" when extracting results; renaming these groups in the pattern would require corresponding code changes.

---

## HttpClientName
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
public const string HttpClientName = "DuckDuckGoSearch"
```


Represents the canonical name used to create or retrieve a named HttpClient for the DuckDuckGo web search integration. Use this constant instead of a string literal to avoid typos and to keep registration and consumption of the HttpClient consistent across the codebase.

## Remarks
This constant centralizes the HttpClient identifier, pairing AddHttpClient(HttpClientName, ...) with IHttpClientFactory.CreateClient(HttpClientName). It reduces drift between registration and usage and makes resilience and base-configuration policies easier to apply per client name. If the name ever changes, updating this single symbol propagates to all call sites that reference it.

## Example
```csharp
// Registration
services.AddHttpClient(HttpClientName, client => {
    client.BaseAddress = new Uri("https://duckduckgo.com/");
    // additional defaults (headers, timeouts, etc.)
});

// Consumption
var client = httpClientFactory.CreateClient(HttpClientName);
```

## Notes
- Using the constant ensures a single source of truth for the client name, preventing mismatches between registration and usage.
- Ensure the named HttpClient is registered in the DI container before any consumer resolves it; otherwise CreateClient(HttpClientName) will fail.

---

## LiteEndpoint
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string LiteEndpoint = "https://lite.duckduckgo.com/lite/"
```


Defines the base URL used to reach DuckDuckGo's Lite search endpoint from the web search tooling. This private constant is consulted whenever the code needs to perform a lightweight DuckDuckGo query, ensuring all calls share the same base address.

## Remarks
Provides a single source of truth for the Lite endpoint used by the web search components. This makes it straightforward to switch to a different host or path in one place without hunting through call sites.

## Notes
- The trailing slash is intentional to ease URL composition; when building full URLs, avoid duplicating slashes.
- Because it is a private constant, it cannot be overridden at runtime; to alter behavior, modify the constant value in code and recompile.

---

## LiteResultLinkRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex LiteResultLinkRegex = new(
        @"<a[^>]*href=""(?<href>[^""]+)""[^>]*class='result-link'[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


LiteResultLinkRegex is a precompiled, private static readonly Regex used to extract the target URL and display text from a hyperlink within the Lite DuckDuckGo search results HTML. It targets an anchor tag with class='result-link' and captures the href attribute as href and the inner text as text. This mirrors the Lite endpoint's flatter HTML structure, where a result's title is followed by a snippet in the next row. The pattern is compiled for performance and uses Singleline to allow matching across line breaks; discovered links are subsequently passed through UnwrapRedirect for safety—the helper is a no-op if the redirect marker is absent.

## Remarks
Centralizes the Lite-result parsing concern in a single, reusable piece of logic. Because it is tailored to a specific HTML shape, any markup changes would require updating the regex or adding fallback parsing; the use of named captures (href, text) simplifies downstream consumption by the caller.

## Notes
- Relies on double-quoted href and single-quoted class attributes; variations will break the match.
- Does not perform redirect unwrapping itself; downstream code must invoke UnwrapRedirect as indicated in the comments.
- Marked as private static readonly to avoid repeated allocations and to keep the pattern inlined with the class' lifecycle.

---

## LiteSnippetRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex LiteSnippetRegex = new(
        @"<td[^>]*class='result-snippet'[^>]*>(?<text>.*?)</td>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


This precompiled, private static Regex field centralizes the extraction of the textual snippet from a DuckDuckGo search result cell. It targets a td element whose class attribute equals 'result-snippet' and captures the inner content in a named group called text. Being static readonly, the Regex is initialized once and reused, and the RegexOptions.Singleline flag allows the captured text to span multiple lines while RegexOptions.Compiled improves parsing performance in hot paths.

## Remarks
- Centralizing the snippet extraction as a precompiled Regex reduces allocations and avoids re-creating the pattern for each match, which helps maintain high performance when processing many results.
- The named capture group text provides a stable, typed access point for the snippet content without manual string manipulation.
- Because this field is private, it is intended as an internal helper for the surrounding web-search parsing logic; external callers should rely on higher-level APIs rather than touching the Regex directly.
- The Static initialization is thread-safe in .NET, so concurrent reads are safe, but the brittleness of HTML structure remains a consideration.

## Example
```csharp
var m = LiteSnippetRegex.Match(htmlFragment);
if (m.Success)
{
    string snippet = m.Groups["text"].Value;
}
```

## Notes
- The pattern assumes the class attribute precisely equals 'result-snippet' (with single quotes) and that the HTML uses a td element as the snippet container; changes to quotes or structure may break the match.
- If you need to parse multiple snippets from a single HTML document, consider using Regex.Matches and iterating over all captures, or prefer a proper HTML parser for robustness.

---

## TagStripRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex TagStripRegex = new(@"<[^>]+>", RegexOptions.Compiled)
```


This private static readonly Regex matches HTML-like tags by looking for substrings that start with '<', contain one or more characters that are not '>', and end with '>'. It’s intended for stripping markup from text sourced from the web by removing the tag portions while leaving the inner text intact. The field is declared static and readonly and uses RegexOptions.Compiled to improve performance by compiling the pattern once and reusing the single instance across the lifetime of the class.

## Remarks
Centralizing this pattern as a private field ensures consistent tag-stripping behavior across all internal usages and avoids repeatedly compiling the same regular expression. Reusing a single compiled Regex also reduces allocations and improves hot-path performance when cleaning large amounts of HTML-like content from search results.

## Notes
- The regex only removes the tag markers; it does not sanitize or remove the content inside tags (e.g., the text inside <div>...</div> remains). If you need to strip elements and their content, you’ll need a more advanced approach.
- It may inadvertently strip angle-bracket text that resembles tags in non-HTML contexts. For input that may contain literal <tag> style fragments, consider escaping or using a more constrained pattern.

---

## UserAgents
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly string[] UserAgents =
    [
        
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
        
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0",
        
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0",
        
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
    ]
```


Contains a small, real-browser User-Agent fingerprint pool used to seed a session's HTTP fingerprint. The values are chosen at session warmup and are held for the duration of that session, with per-request UA rotation avoided to reflect how real browsers behave and to reduce bot-detection signals. The pool is designed to spread fingerprints across deployments and restarts, preventing excessive reuse of a single UA across environments.

## Remarks
This field encapsulates the UA provisioning policy separate from request logic, ensuring a stable identity for the lifetime of a session. By fixing one UA per session, the system avoids UA flicker that can trigger anomaly-detection rules, while distributing fingerprints across deployments minimizes clustering of identical fingerprints. This approach helps replicate realistic user behavior without exposing a rapidly changing UA mid-session, which can be noisy for analytics and more likely to be flagged by defense systems.

## Notes
- The array is private static readonly and initialized once; it is not mutated at runtime.
- To change the UA, a new session must be initialized or the warmup logic must select a different entry; there is no per-request rotation within a single session.
- Keep the list updated to reflect current browser signatures; outdated UA strings can look suspicious or fail to match modern engines.

---
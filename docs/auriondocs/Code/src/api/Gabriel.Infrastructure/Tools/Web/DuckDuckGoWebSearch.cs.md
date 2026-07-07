# DuckDuckGoWebSearch.cs

> **Source:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`

## Contents

- [DuckDuckGoWebSearch](#duckduckgowebsearch)
- [DuckDuckGoWebSearch](#duckduckgowebsearch-1)
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

## DuckDuckGoWebSearch
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class DuckDuckGoWebSearch : IWebSearch
```


A DuckDuckGo-backed implementation of IWebSearch that performs free, no-API-key web searches by scraping DuckDuckGo's public HTML endpoints. It prefers the richer html/ endpoint (for snippets and more metadata) and falls back to the lite/ endpoint when the primary returns no parseable results or appears to be throttling or serving an anomaly page. Use this when you need an out-of-the-box, keyless search provider for development, low-volume, or best-effort use; for production-scale reliability consider a paid search API.

## Remarks
This class encapsulates the pragmatic tradeoffs of scraping a public search engine: no API key or quota is required, but parsing is HTML/regex-driven and therefore brittle to layout changes. It maintains lightweight session state (a chosen real-browser User-Agent and a warmed CookieContainer maintained on the HttpClientHandler) to reduce bot detection; a SemaphoreSlim serializes the first-use "session warm" step so concurrent callers don't duplicate work. The implementation logs diagnostic detail when it detects an "anomaly" page from DuckDuckGo so rate-limiting or CAPTCHA-like responses can be investigated rather than silently returning zero results.

## Notes
- Parsing is intentionally forgiving: failures yield zero results rather than throwing—expect occasional empty responses when DDG's HTML structure changes.
- A single User-Agent string is selected at session warmup and reused for the session (do not expect per-request UA rotation); anomaly detection will reset this session state.
- The HttpClient used must be configured with a CookieContainer on its handler (the class relies on session cookies populated during the homepage pre-warm).

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


DuckDuckGoWebSearch's constructor wires up the service by taking an IHttpClientFactory for creating HTTP clients and an `ILogger<DuckDuckGoWebSearch>` for diagnostics, storing them for use by the instance. Developers typically rely on dependency injection to supply concrete implementations, or substitute mocks in tests.

## Remarks
This constructor follows the dependency-injection pattern: it does not instantiate its collaborators directly, it receives them from the container or test harness. That makes the class easier to test and more flexible in different hosting environments, since HTTP client lifetimes and logging behavior can be swapped without changing the class. The private fields (_httpFactory and _logger) are intended to be used throughout the instance to perform HTTP calls and emit logs.

## Notes
- No argument null-checks are performed; if a null IHttpClientFactory or ILogger is supplied, subsequent usage of _httpFactory or _logger may throw. Ensure the DI container provides non-null instances or add validation.

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


BuildRequest creates a HttpRequestMessage for the given method and URL, prepopulated with the complete set of headers a real Chrome/Firefox navigation would carry. Building headers per-request lets the caller pin or rotate the User-Agent across requests and produce correct Sec-Fetch-* signals for the two navigation phases (the initial entry vs. an in-site navigation). The method selects the User-Agent from _sessionUserAgent when available, or falls back to the first entry in the UserAgents collection if it is null (defensive behavior documented in code). It then applies standard headers: Accept, Accept-Language, DNT, Upgrade-Insecure-Requests, Sec-Fetch-Dest, Sec-Fetch-Mode, and Sec-Fetch-User. For the initial navigation (isInitialNavigation = true) it sets Sec-Fetch-Site to none to reflect a user-typed URL or bookmark open; for subsequent navigations (isInitialNavigation = false) it sets Sec-Fetch-Site to same-site and adds a Referer header pointing at Homepage. The result is an HttpRequestMessage ready to be sent with HttpClient, with navigation-state-sensitive headers that enable realistic server-side handling and testing of navigation paths.

## Remarks
Isolates browser-like header construction from request execution so every request carries explicit navigation state. This makes the transition from an initial warmup/navigation to subsequent intra-site navigation explicit and testable, and it supports session-based UA rotation and site-signal fidelity. By avoiding HttpClient.DefaultRequestHeaders for per-request state, the code ensures header accuracy across requests and makes the intent of each fetch crystal-clear.

## Example
```csharp
// Example usage within the same class (BuildRequest is private)
var initialReq = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/", true);
var subsequentReq = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/?q=docs", false);
```

## Notes
- If _sessionUserAgent is null, the method falls back to UserAgents[0], ensuring a usable User-Agent header even before session initialization completes. 
- Do not rely on HttpClient.DefaultRequestHeaders to convey per-request navigation-state headers; BuildRequest ensures the correct combination of headers for each request.
- The isInitialNavigation flag controls both Sec-Fetch-Site and the presence of Referer, which is important for servers that differentiate between a user-entered navigation and an in-site navigation.


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


Converts a raw string that may contain HTML into clean, plain text by HTML-decoding entities, stripping HTML tags with a regex, and trimming whitespace. This private helper consolidates the common post-processing step used when handling text extracted from web results, ensuring a consistent, display-friendly output instead of duplicating decoding, tag removal, and trimming logic.

## Remarks
By encapsulating this sequence behind CleanText, the codebase can adjust how text is sanitized in one place (e.g., changing the regex or decoding behavior) without touching every call site. It relies on TagStripRegex and WebUtility to ensure HTML entities are decoded correctly and markup is removed before the text is consumed by higher-level components.

## Notes
- No null-check on input; pass a non-null string to avoid runtime exceptions.


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


DetectAnomalyPage determines whether the provided HTML contains known anti-bot / CAPTCHA-style markers (such as anomaly_modal, anomaly-modal, anomaly.js, Cloudflare's "Just a moment" interstitial, cf-mitigated, cf-browser-verification, or __cf_chl_). It returns true when any of these markers are found, signaling that the page is an anomaly page rather than a normal user-facing result. Centralizing this logic in a single internal helper allows callers to either fall back to a sibling endpoint or surface a diagnostic instead of misclassifying the page as a valid query result.

The method is private static, so it's intended for internal use within the class and not for public consumption.

## Remarks
This predicate encapsulates recognition of anti-bot blocks across multiple sources, ensuring a consistent handling path when a page is not a normal result. It reduces duplication by centralizing marker checks and makes it straightforward to extend with new markers as needed.

## Notes
- The method assumes a non-null html input; passing null will throw at runtime.
- The detection set is not exhaustive; new anomaly markers may be added as bot-protection pages evolve.

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


Ensures a one-time warm-up of the DuckDuckGo session for the current instance by performing a minimal GET to the homepage to acquire session cookies and seed the CookieContainer. A random User-Agent from the UserAgents collection is selected and used for the preliminary navigation, mirroring typical browser behavior. The body of the response is ignored—the effect of the call is the cookies collected by the HttpClient's handler, which apply to both html.* and lite.* subdomains for subsequent requests. The warm-up is guarded by a per-session lock and a _sessionWarmed flag so the operation runs only once; later invocations return immediately. If the warm-up fails with a non-cancellation exception, a warning is logged and the actual search proceeds without homepage cookies. When the attempt completes (success or recoverable failure), _sessionWarmed is set to true and the lock is released.

## Remarks

Conceptually, this isolates the session bootstrap from the main search path, ensuring subsequent requests start with a predictable cookie state. By centralizing the one-time initialization, the code reduces race conditions and cookie-state variability across concurrent searches. The design adopts a best-effort bootstrap: if the warm-up cannot reach the homepage, the actual search still proceeds, just without the homepage cookies.

## Notes

- The warm-up is best-effort; a non-fatal failure logs a warning but does not block a search.
- Cookies are captured by the HttpClient's cookie container and are scoped to .duckduckgo.com, applying to both html.* and lite.* subdomains for subsequent requests.
- If UserAgents is empty, Random.Shared.Next(UserAgents.Length) will throw; ensure the UserAgents collection is non-empty.

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


FetchAsync is a private asynchronous helper that retrieves the HTML content for a DuckDuckGo search by constructing a GET request with the provided query, inserting a small randomized delay to mimic human reading cadence and help avoid bot-detection signals, and then returning the response body as text. If the HTTP response indicates failure, it logs a warning and throws HttpRequestException to propagate the error to callers.

## Remarks
FetchAsync encapsulates the boundary between network I/O, bot-detection considerations, and error handling within the DuckDuckGo web search path. By centralizing delay, URL construction, and failure propagation, it keeps the higher-level search workflow focused on result processing rather than request choreography. The private scope signals that this is an implementation detail of the search component.

## Notes
- The method respects the provided CancellationToken; Task.Delay and HttpClient.SendAsync are cancelled if ct is triggered.
- It encodes the search query with Uri.EscapeDataString and builds a URL with q=<encoded>&kl=us-en to simulate a real navigation context.
- Non-success responses log a warning and throw HttpRequestException to ensure callers can react to HTTP errors consistently.

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


This private helper returns the first n characters of the given string, or the entire string if it is shorter than n. Use it when you need a compact prefix for display or logging without allocating a new string when the input already fits.

## Remarks
Small, private utility that centralizes the common 'prefix up to a length' pattern. It simplifies callers by handling the length check and the slicing in one place, and it will keep the semantics consistent wherever a truncated representation is needed. Because it returns the original string if it already fits, it avoids unnecessary allocations.

## Notes
- Null input will throw a NullReferenceException since there is no null check.
- Negative n will throw due to the range operator when slicing.
- The method returns the original string when s.Length <= n; callers relying on a newly allocated string should be aware.

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


Parses the HTML returned by the DuckDuckGo-style search endpoint to produce up to a specified number of WebSearchResult entries. The method iterates blocks matched by HtmlResultBlockRegex, reads the block body, locates a title and href via HtmlTitleLinkRegex, HTML-decodes the href, unwraps potential redirects, and optionally captures a snippet. Only results with a non-empty title and URL are kept, and parsing stops once the requested limit is reached. The outcome is a `List<WebSearchResult>` suitable for presentation or further processing.

## Remarks
The function centralizes the HTML-to-domain transformation for search results, shielding callers from the exact HTML structure by expressing it through pre-defined regexes. It relies on HtmlResultBlockRegex, HtmlTitleLinkRegex, and HtmlSnippetRegex to locate pieces of each result, and on helper methods like CleanText and UnwrapRedirect to sanitize text and normalize URLs. Because the parsing depends on the HTML shape, changes to the endpoint's markup or the regexes may require corresponding updates elsewhere in the codebase.

## Notes
- Blocks missing a title or URL are skipped gracefully, which means some results may be omitted if the HTML isn't as expected.
- The method enforces the provided limit, so callers should account for potential truncation of results if the page contains more than the requested amount.

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


ParseLiteEndpoint parses the HTML returned by the Lite search endpoint and builds a list of WebSearchResult entries up to the specified limit by pairing link elements with their corresponding snippets. It uses LiteResultLinkRegex and LiteSnippetRegex to locate links and snippets, decodes and unwraps the href to obtain a clean URL, and uses a sanitized title; results with empty title or URL are skipped. If a snippet exists at the same index as a link, it is included; otherwise the snippet is an empty string. The code note explains that the snippet is aligned by index with the link in the lite layout, so a missing snippet can drift but the URL and title remain correct.

## Remarks
This method is a focused parsing helper that translates the Lite endpoint's HTML into the WebSearchResult domain model, encapsulating the index-based alignment between link and snippet entries used by the lite layout.

---

## ResetSession
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private void ResetSession()
```

**Returns:** `void`


Resets the current session state to force a fresh session on the next search. When invoked (as indicated by the inline comment), it clears the warmed flag and the chosen User-Agent so that the subsequent SearchAsync rebuilds the session with a freshly-picked UA and triggers another homepage round-trip, refreshing the cookie jar with whatever DDG hands back this time and subtly shifting the fingerprint.

## Remarks
This method acts as a focused recovery mechanism: it isolates the session reinitialization from the normal request path to improve resiliency after an anomaly page. By mutating internal state rather than performing a full reconfiguration of the networking layer, it ensures that the next operation begins with a clean slate, enabling a fresh homepage round-trip and updated cookies/identity signals on subsequent requests. The intent is to adapt to potentially fingerprint-adjusting responses from DDG while minimizing disruption to the surrounding flow.

## Notes
- Private scope means this method is intended for use only within its containing class.
- It performs in-memory state mutation; no network activity occurs within ResetSession itself.
- It should be used as part of the anomaly-handling flow to trigger a future reinitialization rather than as a general reset mechanism.

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


SearchAsync asynchronously returns a read-only list of WebSearchResult for a given query by trying DuckDuckGo's html/ endpoint first and parsing the results; if that endpoint yields nothing or is blocked, it falls back to the lite/ endpoint. If both endpoints signal an anomaly, it resets the session and throws an InvalidOperationException with guidance to use Brave/Tavily API keys; if no results are produced at all, it returns an empty list.

## Remarks
SearchAsync encapsulates the resilience and endpoint switching required for lightweight web search scraping. It hides the details of two parallel endpoints, anomaly detection, and session management from callers, providing a single, reusable entry point that returns results when available and gracefully handles common anti-bot scenarios through logging and controlled fallbacks.

## Notes
- If both html/ and lite/ endpoints are blocked, the method throws an InvalidOperationException with guidance to switch to a configured alternative provider and API key.
- When both endpoints return 200 with no parseable results, the method returns an empty `IReadOnlyList<WebSearchResult>` to signal a genuine empty result for the query (not an error).
- Callers should be prepared to handle potential exceptions (e.g., InvalidOperationException) and may implement backoff or alternate providers as recommended in the exception message.

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


UnwrapRedirect decodes a DuckDuckGo redirect wrapper and returns the real target URL. It looks for the uddg= parameter inserted by the wrapper, decodes the contained URL-encoded value, and returns it. If the wrapper is not present, the function forwards the original href; for protocol-relative URLs that start with //, it normalizes them to https://.

## Remarks

Encapsulates brittle URL-wrapping logic behind a small, deterministic helper, making higher-level code easier to test. The method is defensive: any failure to decode results in returning the input URL unchanged, preventing exceptions from leaking. It relies only on basic System APIs (StringComparison, Uri) and does not perform network I/O or clever parsing beyond the first uddg occurrence.

## Notes

- Only the first uddg= occurrence is considered; subsequent parameters are ignored.
- If decoding fails, Uri.UnescapeDataString may throw; the call is wrapped in a try/catch and the original href is returned.
- Protocol-relative URLs are normalized to HTTPS by prefixing with https:.

---

## Homepage
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string Homepage = "https://duckduckgo.com/"
```


Homepage is a private constant string that stores the canonical DuckDuckGo homepage URL used by the internal web search implementation. By centralizing the URL in a single constant, the code avoids duplicating the literal and ensures consistent navigation targets when constructing requests or links. Because it is private and a compile-time constant, this value is an internal detail of the web search component and is inlined at compile time.

## Remarks

Centralizing the homepage URL as a private constant ensures a single source of truth for the DuckDuckGo homepage within the web search flow, reducing the risk of inconsistent URLs. It also isolates the value from the public surface, so external callers cannot depend on or mutate it. If the URL ever needs to vary by environment, this pattern makes the change localized to this member (or a follow-up internal abstraction).

## Notes

- Not accessible publicly; as a private const, the value is compiled into every use site and cannot be changed at runtime. Updating it requires recompilation of the consuming assemblies.
- If tests or external components need to know the URL, introduce a test seam or configuration instead of depending on internal constants in non-test code.

---

## HtmlEndpoint
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string HtmlEndpoint = "https://html.duckduckgo.com/html/"
```


HtmlEndpoint is the explicit HTML DuckDuckGo search URL used by internal requests to fetch rich HTML results. It is defined as a private constant and pinned to the html.duckduckgo.com/html/ endpoint to guarantee that all requests hit the correct host, independent of HttpClient.BaseAddress configuration.

## Remarks
Centralizes the HTML endpoint to prevent accidental use of the lite URL and to keep HTML results stable. By using an absolute URL, it ignores any HttpClient.BaseAddress and guarantees the request goes to the intended host. The private scope and dedicated constant make the HTML path explicit and resilient to misconfiguration.

## Notes
- Private visibility means external code cannot rely on it; modifications impact internal behavior only.
- The value is a hard-coded HTTPS URL that ensures a secure, consistent endpoint and host.
- If DuckDuckGo changes its endpoint structure, this constant should be updated accordingly to maintain correct behavior.

---

## HtmlResultBlockRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlResultBlockRegex = new(
        @"<div[^>]*class=""[^""]*\bresult\b[^""]*""[^>]*>(?<body>.*?)(?=<div[^>]*class=""[^""]*\bresult\b|<div[^>]*id=""bottom_spacing""|\Z)",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


HtmlResultBlockRegex is a precompiled regular expression used to extract the inner HTML of a single search result block from a DuckDuckGo-style results page. It matches a div whose class contains the word 'result' and exposes the block's content via a named capture group called 'body', stopping before the next result or the end of the input.

## Remarks
This field is a private static readonly Regex, ensuring the pattern is compiled once and reused across parsing invocations for performance. The expression relies on server-generated markup and centralizes parsing logic behind a stable pattern; changes to the HTML structure may require updating this single regex to maintain correct extraction. By isolating the HTML-block parsing, downstream code can operate on the extracted 'body' content without re-implementing DOM-like traversal.

## Notes
- The regex assumes a fairly stable HTML structure where result entries are divs with a class containing 'result'; any markup change could invalidate the pattern. 
- It captures using a non-greedy approach to stop at the next result block or the bottom spacing marker, but deeply nested or differently structured blocks may still cause misses.

---

## HtmlSnippetRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlSnippetRegex = new(
        @"class=""[^""]*\bresult__snippet\b[^""]*""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


HtmlSnippetRegex is a precompiled, private static readonly Regex used to pull the visible snippet text from DuckDuckGo search result HTML. It matches an anchor tag with a class containing result__snippet and captures its inner text, letting the surrounding code extract the snippet in a single pass instead of brittle manual string slicing or ad-hoc parsing.

## Remarks
This symbol centralizes the HTML structure dependency of search results into a dedicated, reusable piece, reducing duplication and making future changes localized. The use of RegexOptions.Compiled and Singleline indicates a performance-oriented parsing path that handles multi-line HTML content while avoiding repeated regex compilation. Keeping it private prevents incidental misuse and enforces a single, consistent extraction path within the class.

## Notes
- The pattern relies on DuckDuckGo's markup and may require updates if the HTML changes.
- Because HtmlSnippetRegex is private, tests should exercise the public parsing surface rather than the field itself.

---

## HtmlTitleLinkRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlTitleLinkRegex = new(
        @"<a[^>]*class=""[^""]*\bresult__a\b[^""]*""[^>]*href=""(?<href>[^""]+)""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


This private static readonly Regex is used to parse the title link from a DuckDuckGo search results page. It matches an anchor tag whose class attribute contains the word result__a and captures the href value and the link text into named groups href and text. The Regex is constructed once, stored as a static, compiled instance, and reused to efficiently extract link targets from multiple HTML fragments.

## Remarks
This symbol centralizes the specific HTML pattern used to identify search-result anchors, providing a reusable, high-performance utility for extracting the destination URL and its display text from DuckDuckGo HTML responses. By compiling and caching the pattern, it avoids repeated allocations during repeated parsing. Because it relies on the page’s markup, any change to the HTML structure may require updating the pattern in one place to restore correct behavior.

## Notes
- Assumes the anchor uses a double-quoted href attribute and includes the class containing result__a; variations may cause no match.
- It is a parsing heuristic, not a full HTML parser; markup changes could break the regex.
- Always verify m.Success before accessing Groups to avoid runtime errors.

---

## HttpClientName
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
public const string HttpClientName = "DuckDuckGoSearch"
```


Public constant string HttpClientName defines the named HttpClient used by the DuckDuckGo web search integration. By exporting the name as a constant, the codebase can register and retrieve the same HttpClient instance without scattering the literal string across files. This pattern is typically leveraged with HttpClientFactory or DI when configuring a client (for example base address, default headers, or timeouts) for DuckDuckGo web requests.

## Remarks
Centralizes the identifier for the DuckDuckGo HTTP client, reducing the risk of typos and enabling consistent configuration across the application. It acts as a single source of truth for the HttpClient's name, tying together registration and retrieval points. Because it is a public compile-time constant, changes to the name are visible at compile time to dependents, which emphasizes the need for coordinated versioning.

## Notes
- Because HttpClientName is a const, its value is baked into referencing assemblies. Changing it requires recompilation of dependents; otherwise, runtime lookups for the named client may fail.

---

## LiteEndpoint
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string LiteEndpoint = "https://lite.duckduckgo.com/lite/"
```


This private compile-time constant stores the base URL for the DuckDuckGo Lite search endpoint. It serves as the single source of truth for the endpoint used by the web search logic when constructing requests to the Lite interface, ensuring consistent and centralized URL usage within the component.

## Remarks
An internal design choice: because this field is private and declared as const, its value is baked into the compiled assembly and cannot be changed at runtime. If you anticipate the need for different endpoints per environment, prefer configuration or dependency injection rather than altering this constant. Keeping the endpoint private also encapsulates the URL details, letting the rest of the class focus on request construction rather than URL management.

## Notes
- Fixed at compile time; the value cannot be mutated at runtime without recompiling.
- Not part of the public API; to support multiple environments, move to a configurable provider or injectable option.

---

## LiteResultLinkRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex LiteResultLinkRegex = new(
        @"<a[^>]*href=""(?<href>[^""]+)""[^>]*class='result-link'[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


Precompiled regular expression used by the Lite endpoint parser to extract direct result links from the DuckDuckGo HTML snippet. It matches an anchor tag with href and class='result-link', capturing the URL as href and the display text as text; the Lite endpoint uses a flatter structure, and the URL is passed through UnwrapRedirect for safety (a no-op if the marker is absent).

## Remarks
Because this pattern is tightly coupled to the Lite HTML markup, changes to the anchor's attributes or structure can break matches and require updates to the regex. Declared as static readonly and compiled, it is initialized once and reused across parsing operations for performance. UnwrapRedirect is applied to normalize redirects when a link is matched, ensuring downstream consumers receive canonical URLs.

## Notes
- Tightly coupled to the Lite endpoint's HTML: changes to the anchor markup (quotes, attributes, ordering) will likely break the match.
- href must be enclosed in double quotes and class must be exactly result-link with single quotes; variations will fail to match.
- The text capture uses a non-greedy match; nested tags inside the anchor can affect the captured text.

---

## LiteSnippetRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex LiteSnippetRegex = new(
        @"<td[^>]*class='result-snippet'[^>]*>(?<text>.*?)</td>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


LiteSnippetRegex is a private static readonly Regex that defines how to capture the textual snippet from a DuckDuckGo search result HTML. It targets a table cell (<td>) with a class attribute of 'result-snippet' and exposes the captured inner content as a named group 'text'. The pattern is compiled and uses Singleline mode to efficiently parse HTML fragments that may span multiple lines, and its instance is shared across the class to avoid repeated Regex construction.

## Remarks
Centralizes snippet-extraction logic in one place, simplifying maintenance if the markup changes. The static readonly field ensures a single compiled Regex is reused across calls, improving performance in hot paths that process many results. While convenient, this approach is brittle to markup changes; if the page structure or class name changes, the regex may stop matching and require an update or a switch to a proper HTML parser.

## Notes
- Fragile HTML parsing: changes to the DuckDuckGo markup or the 'result-snippet' class can break the regex.
- Performance and initialization: RegexOptions.Compiled and static initialization improve repeated-match performance but incur a one-time initialization cost; ensure this aligns with startup requirements.

---

## TagStripRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex TagStripRegex = new(@"<[^>]+>", RegexOptions.Compiled)
```


Represents a precompiled regular expression used to strip simple markup tags from text. TagStripRegex is a private static readonly field initialized with the pattern <[^>]+> and compiled for performance. It is intended to be reused across invocations to efficiently remove tag-like substrings from inputs, typically via TagStripRegex.Replace(input, string.Empty). This is not a full HTML parser; it handles straightforward tags but may not cover edge cases such as comments, CDATA sections, or attributes containing the closing angle bracket.

## Remarks
TagStripRegex provides a single shared Regex instance to avoid repeated construction, which improves performance in string-sanitization workflows. Because the field is private and readonly, the containing class controls how and when tag stripping occurs, while still benefiting from the speed of a precompiled pattern. Reusing this single instance across method calls avoids extra allocations and regex compilation overhead.

## Notes
- The pattern <[^>]+> is simplistic and may fail on edge cases such as HTML comments, CDATA, or attribute values containing a '>' character.
- Null inputs will cause exceptions if you directly apply Replace; ensure input is non-null or perform a null check before using TagStripRegex.Replace(input, string.Empty).

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


Contains a small, fixed pool of real-browser User-Agent strings used by the HTTP client. The intent is not to rotate the User-Agent on every request; real browsers don't flip UA mid-session, and rapid UA changes within a single session can signal automation. A single UA is selected during session warmup and is retained until the session is reset by anomaly-detection logic. The pool therefore aids in spreading fingerprint diversity across deployments or restarts without introducing per-request churn.

## Remarks
This field is declared private, static, and readonly, which means the pool is shared across all usages within the class, and the reference cannot be reassigned at runtime. The actual strings remain immutable from external code, ensuring a stable fingerprint for the duration of a session. If future requirements demand dynamic UA rotation or per-request variation, consider extracting this into a UA-provider abstraction rather than embedding the logic directly in the field.

## Notes
- The field is private and readonly; external code cannot observe, mutate, or replace the pool at runtime. Any change would require code changes to the provider.


---
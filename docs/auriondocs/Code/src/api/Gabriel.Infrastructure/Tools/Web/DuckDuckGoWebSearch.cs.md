# DuckDuckGoWebSearch.cs

> **Source:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`

## Contents

- [DuckDuckGoWebSearch](#duckduckgowebsearch)
- [DuckDuckGoWebSearch (constructor)](#duckduckgowebsearch-constructor)
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


A lightweight, no-API-key IWebSearch implementation that queries DuckDuckGo by scraping its public HTML endpoints. It issues searches first against the richer html.duckduckgo.com/html/ endpoint and, if that returns no parseable results, falls back to lite.duckduckgo.com/lite/. Use this implementation when you need an out-of-the-box web search provider without an API key and can tolerate HTML-driven parsing and occasional rate-limiting behavior.

## Remarks
DuckDuckGoWebSearch is built to be pragmatic rather than strictly robust: it prefers ease-of-use (no key, no quota) and diagnostic transparency over the guarantees of a paid API. To reduce bot detection it picks a single realistic User-Agent for the lifetime of a session, pre-warms the session by visiting DuckDuckGo’s homepage so the HttpClientHandler cookie container is populated, and deduplicates concurrent warmups with a semaphore. The class parses HTML with forgiving regexes (parse failures yield zero results instead of exceptions) and logs detailed diagnostics when DuckDuckGo returns an anomaly/anti-bot page so operators can understand and react to rate-limiting.

## Example
```csharp
// Resolve an IWebSearch (for example via DI) and perform a search.
// IWebSearch.SearchAsync is the public entrypoint implemented by DuckDuckGoWebSearch.
CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
IWebSearch webSearch = /* obtain from DI or construct with required services */ null!;
IReadOnlyList<WebSearchResult> results = await webSearch.SearchAsync("how to parse html in c#", 5, cts.Token);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}");
}
```

## Notes
- Parsing is HTML/regex-driven; structural changes to DuckDuckGo’s pages can cause zero results rather than exceptions.
- DuckDuckGo may return an anomaly (bot-detection) page; the implementation detects and logs this and will ResetSession() to pick a fresh UA/cookie state when appropriate.
- Session warmup is protected by a SemaphoreSlim; first-call latency may be higher due to the initial homepage request, and concurrent callers are deduped rather than causing multiple warmups.

---

## DuckDuckGoWebSearch (constructor)
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


DuckDuckGoWebSearch's constructor wires the object with its essential dependencies: an IHttpClientFactory for creating HttpClient instances and an `ILogger<DuckDuckGoWebSearch>` for logging. This constructor is designed for dependency-injected scenarios or for tests where you provide mocks or fakes for those services; once constructed, the instance stores these dependencies for its HTTP operations and log output.

## Remarks
- The constructor does not perform null checks; ensure non-null dependencies are supplied (DI containers typically enforce this, but be aware of test or manual setups).
- Using `ILogger<DuckDuckGoWebSearch>` ties log messages to this class, enabling focused filtering and consistent formatting. IHttpClientFactory promotes efficient HttpClient usage and centralized HTTP configuration, reducing socket exhaustion risks.

## Example
```csharp
// Example: direct construction (useful in tests)
IHttpClientFactory httpFactory = /* obtain from DI or mocks */;
ILogger<DuckDuckGoWebSearch> logger = /* obtain from DI or mocks */;
var search = new DuckDuckGoWebSearch(httpFactory, logger);
```

## Notes
- No parameter validation is performed here; rely on your DI setup to supply valid instances.
- Ensure DI configuration provides appropriate HttpClientFactory and logging capabilities for production scenarios.

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


BuildRequest creates a new HttpRequestMessage and populates it with a browser-like header set for each request. It ensures the User-Agent can be rotated per session, and it fills Accept, Accept-Language, DNT, Upgrade-Insecure-Requests and Sec-Fetch-* headers; crucially, it differentiates initial navigation (no Referer, Sec-Fetch-Site: none) from subsequent in-site navigations (Sec-Fetch-Site: same-site and a Referer).

## Remarks
BuildRequest centralizes per-request header construction to avoid leaking per-session state into HttpClient.DefaultRequestHeaders, and to preserve accurate navigation semantics for server-side behavior and analytics. It also enables session pinning of the User-Agent and correct variation of Sec-Fetch-Site between navigation phases, which helps mimic real browser traffic more closely and reduces accidental fingerprinting.

## Notes
- Per-request header construction is deliberate to allow session-specific UA rotation and correct Sec-Fetch-* values for initial vs subsequent navigations; do not rely on HttpClient.DefaultRequestHeaders for this scenario.
- The UA fallback uses UserAgents[0] when the session UA is not yet populated; ensure the session initialization populates _sessionUserAgent before the first BuildRequest call to avoid fingerprint drift.

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


Converts a raw HTML string into plain text by decoding HTML entities, stripping HTML tags, and trimming whitespace. It is intended for scenarios where you need readable text extracted from HTML sources rather than markup, such as displaying summaries or processing web content where only the text matters.

## Remarks
By centralizing the HTML-to-text normalization, CleanText ensures consistent behavior across all call sites and reduces duplication of the decode-strip-trim sequence. It relies on two collaborators: TagStripRegex (for removing tags) and WebUtility (for decoding entities). Changes to those components will propagate to every consumer of CleanText, making this small helper a stable, single point of truth for this transformation.

## Notes
- The tag-stripping is regex-based and may not cover every edge case of malformed or complex HTML; for strict parsing, consider a dedicated HTML parser.
- The method is private static; external callers cannot call it directly. If external reuse is needed, consider adding a public wrapper.
- The order of operations is intentional: first decode entities, then remove tags. This ensures encoded markup like &lt;tag&gt; is treated as text, not as an actual tag after decoding.

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


DetectAnomalyPage is a small helper that inspects an HTML page to determine whether the response is an anti-bot or CAPTCHA-style interstitial produced by Cloudflare and related services. It looks for known markers across multiple blocks (such as anomaly_modal, anomaly-modal, anomaly.js, Just a moment, cf-mitigated, cf-browser-verification, and __cf_chl_) and returns true when any are found. Callers can then fall back to an alternative endpoint or surface a clear diagnostic rather than treating the page as a normal query result.

## Remarks
This predicate centralizes the detection logic for Cloudflare-style blocks, so callers don't need to duplicate string checks across code paths. Keeping the checks in one place makes it easier to evolve the markers as the upstream pages evolve, and it yields a simple boolean that downstream code can branch on. By consolidating the marker checks, it reduces duplication and clarifies intent. The method returns a boolean, serving as a gate for fallback or diagnostic behavior rather than content parsing.

## Notes
- The function uses simple substring checks; it does not perform full HTML parsing, so it may miss block indicators not included in the marker list.
- The comparisons use StringComparison.Ordinal to enforce a culture-invariant, fast check and avoid accidental locale-sensitive mismatches.
- As Cloudflare and related pages evolve, you may need to extend the marker set to keep detection accurate.

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


Ensures the DuckDuckGo session is primed by performing a one-time GET of the homepage to establish cookies and set a session-wide User-Agent. The operation is guarded so it runs only once per session and is serialized with a lock to avoid races between concurrent callers; after the first successful (or attempted) warm-up, subsequent calls return immediately.

During the warm-up, a UA is selected from the available pool and used for the request. The response body is ignored; cookies are collected by the underlying cookie jar and are then applied to subsequent requests for both the html.* and lite.* subdomains. A non-fatal outcome is intentional: if the warm-up fails, the real search request will still proceed, just without the session cookies, and a warning is logged to aid diagnoseability.

## Remarks
This abstraction centralizes session priming so the rest of the search path can rely on a pre-initialized cookie-backed session and a stable User-Agent, without bogging down the main search flow with setup concerns. The UA is chosen once per session to resemble a consistent browser fingerprint, while the cookie jar is allowed to accumulate cookies across the initial navigation. The design prefers resilience: failures to warm up do not block or crash the search path, they merely omit the session cookies for that run.

## Notes
- The warm-up is a best-effort, non-fatal operation; if it throws (other than OperationCanceledException), a warning is logged and the search proceeds without session cookies.
- The method uses a private lock to guarantee only one actual warm-up per session; other concurrent callers await the lock and then observe the warmed state.
- Cookies are captured from the homepage response and scoped to .duckduckgo.com so they apply across both html.* and lite.* endpoints.
- A canceled warm-up leaves _sessionWarmed unset, allowing a subsequent attempt to occur if the token is used again.


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


FetchAsync is a private helper that retrieves the DuckDuckGo results for a given query. It first waits a short, randomized delay to mimic human interaction, then issues a GET request constructed from the provided URL and query parameters, and finally returns the response body as a string. If the request fails, the method logs a warning and throws HttpRequestException to surface the error to callers.

## Remarks
This abstraction centralizes the HTTP invocation for the DuckDuckGo search flow, isolating request creation, timing, error handling, and content retrieval behind a single method. It ensures consistent behavior across requests and makes the higher-level search orchestration easier to reason about. The method honors a CancellationToken to allow callers to cancel the operation and relies on a BuildRequest helper to construct the HttpRequestMessage; the caller receives the raw response content as a string for subsequent parsing or rendering.

## Notes
- The 200–1200 ms delay is implemented to resemble natural user pacing; tests should tolerate this nondeterminism or allow delay bypassing if deterministic behavior is required.
- On non-success HTTP responses, the method logs a warning and throws HttpRequestException including the URL and status code; callers should handle this exception to implement retry or user-facing failure handling.
- The request URL is built as fullUrl = url + "?q=" + Uri.EscapeDataString(query) + "&kl=us-en"; if the input URL already contains a query string, this naive concatenation could produce an invalid URL, so callers should ensure the base URL is URL-appropriate before invoking.

## Dependencies
- HttpRequestException
- Task
- Random
- Shared
- Uri
- HttpMethod
- Content

## Dependency APIs
- property Content (src/api/Gabriel.Core/Entities/Message.cs)


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


FirstChars returns the prefix of length n from the input string s, or the entire string if s is shorter than n. It avoids allocation when the string already fits by returning the original string reference; otherwise it uses the range operator to produce the substring.

## Remarks
This helper encapsulates a simple, explicit operation: obtain up to n characters from the start of s, avoiding allocations when the input already fits. It centralizes the common prefix logic so call sites don't need to reimplement range-based slicing and it makes the boundary behavior (returning the whole string if shorter) explicit.

## Notes
- Negative values for n will throw due to the range operator; ensure n is non-negative before calling.
- When n >= s.Length, the method returns the original string reference without allocation, which can affect reference equality semantics in callers.
- The signature implies s is non-null; if nulls could appear, callers should guard accordingly in surrounding code.

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


Parses the HTML produced by the DuckDuckGoWebSearch endpoint and constructs a `List<WebSearchResult>` up to a specified limit. It iterates over result blocks matched by HtmlResultBlockRegex, extracts the block body, uses HtmlTitleLinkRegex to locate the link title and href, HTML-decodes the href, cleans the title text, and unwraps any redirects. If both a non-empty title and URL are produced, an optional snippet is captured via HtmlSnippetRegex (falling back to an empty string when absent) and a new WebSearchResult(title, url, snippet) is appended. The method stops once the limit is reached or no more blocks remain.

## Remarks
Centralizes the HTML parsing for search results in a single, reusable helper. It encapsulates the sequence of regex-driven extraction, HTML decoding, and text normalization, so call sites don't need to repeat this logic. By operating on a per-call List, it avoids shared mutable state and makes behavior predictable during unit testing.

## Notes
- The method is private and static, so changes to its parsing rules or to the static regex fields may require corresponding adjustments elsewhere in the class.
- If the HTML blocks frequently yield missing or invalid titles/URLs, the resulting list may be sparser than the input indicates.
- The limit parameter prevents unbounded allocations; callers should choose a sensible cap to balance performance and completeness.

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


Parses the HTML produced by the lite endpoint and returns a list of WebSearchResult instances up to the specified limit. It uses LiteResultLinkRegex to locate link elements and LiteSnippetRegex to capture optional snippets, decodes HTML-encoded href values with WebUtility.HtmlDecode, and unwraps potential redirects via UnwrapRedirect. Titles are extracted via CleanText and entries with empty or whitespace-only titles or URLs are skipped. Results are appended in document order; when a snippet is unavailable, an empty string is used, but the corresponding title and URL remain aligned with their snippet. This private helper centralizes the translation of lite-endpoint HTML into a typed collection that callers can consume without parsing HTML themselves.

## Remarks
Isolates the lite-endpoint parsing from higher-level search orchestration, enabling swapping in alternative lite layouts without changing consumer code. The index-based pairing of links and snippets ensures the URL/title relationship remains correct even when snippets drift or are missing. This abstraction also limits HTML parsing concerns to a single location, simplifying testing and maintenance.

## Notes
- The limit parameter must be non-negative; passing a negative limit will throw when constructing the List.
- The method is private; external code must use a public wrapper to exercise lite-endpoint parsing.
- Reliance on LiteResultLinkRegex and LiteSnippetRegex means changes to the underlying lite HTML or regex patterns may require updates to this parser.

---

## ResetSession
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** method

```csharp
private void ResetSession()
```

**Returns:** `void`


Resets internal session state after DDG hands an anomaly page. It clears the warmed flag and the current user agent so the next SearchAsync rebuilds the session with a freshly-picked UA, triggering another homepage round-trip and refreshing cookies and fingerprint.

## Remarks
This private helper centralizes the session invalidation step in the anomaly-handling path, ensuring a clean slate for the subsequent request. By dropping the warmed state and UA, the flow guarantees a fresh browser context on the next cycle, helping adapt to evolving responses from DDG and maintain the integrity of the session lifecycle.

## Notes
- Not thread-safe: concurrent invocations may race on _sessionWarmed and _sessionUserAgent. Synchronize or constrain usage to a single execution flow when needed.

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


SearchAsync coordinates a two-endpoint DuckDuckGo web search. It starts by creating an HTTP client, ensuring a session is established, and querying the rich html/ endpoint to obtain results with snippets and redirect-wrapped URLs. If the html/ response is detected as an anomaly, it is logged and the method falls back to the lighter lite/ endpoint. When the html/ response is not anomalous, parsed results are returned immediately if any are found; otherwise an informational log notes the absence of parseable results.

If the primary path yields no usable results, the method fetches from the lite/ endpoint. As with the primary path, anomalies are logged and non-empty parseable results are returned when available; otherwise an informational log records zero parseable results from lite/.

If either endpoint reports an anomaly/block (detected via DetectAnomalyPage), the session is reset and an InvalidOperationException is thrown with guidance to switch to a managed toolchain (e.g., Brave or Tavily) and provide the corresponding API key for reliable web search. This aligns with the idea that free scraping endpoints can rate-limit or block residential/datacenter IPs, requiring a different approach for production reliability.

When both endpoints deliver 200 responses but no parseable results, an empty `IReadOnlyList<WebSearchResult>` is returned, signaling a genuine absence of results for the query.

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


UnwrapRedirect centralizes the logic for recovering the final destination URL from a DuckDuckGo redirect wrapper. It searches for the uddg= parameter and, when found, decodes its value with Uri.UnescapeDataString to return the real target URL. If the wrapper pattern is not present, it preserves the original href; however, protocol-relative URLs (starting with //) are rewritten to https:// to yield an absolute, modern URL. If decoding fails for any reason, the original href is returned unmodified. This function is intentionally small and defensive, ensuring callers always receive a usable URL without throwing.

## Remarks
UnwrapRedirect exists to centralize the logic for extracting the real URL from DuckDuckGo's redirect wrapper. It encapsulates detection of the uddg parameter, decoding of its value, and a safe fallback when the wrapper is absent or decoding fails. This keeps downstream code focused on URL processing rather than wrapper-specific quirks and ensures protocol-relative URLs are normalized to absolute HTTPS URLs when encountered.

## Notes
- Uses ordinal string comparisons to avoid culture-specific quirks when detecting the wrapper and protocol-relative form.
- Decoding errors are swallowed; in such cases the original href is returned to maintain non-throwing behavior.

---

## Homepage
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string Homepage = "https://duckduckgo.com/"
```


Stores the base URL for the DuckDuckGo web search endpoint used by the internal web search logic. As a private const, it centralizes the endpoint value so the class can reliably build full request URLs without scattering the literal string throughout the codebase.

## Remarks
Centralization reduces duplication and guards against typos when constructing URLs. The private visibility enforces encapsulation, while the const ensures the value is known at compile time and can be inlined for performance.

## Notes
- Because it is private, external code cannot reference or override this value.
- Because it is a compile-time constant, any change requires recompilation of all code that references it; runtime reconfiguration is not possible through injection.

---

## HtmlEndpoint
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string HtmlEndpoint = "https://html.duckduckgo.com/html/"
```


HtmlEndpoint is a private constant string that stores the absolute URL of DuckDuckGo's HTML search endpoint. It is the primary target for obtaining rich HTML search results in the web search integration, pointing to https://html.duckduckgo.com/html/ regardless of any HttpClient.BaseAddress configuration. This explicit separation from the lite endpoint (https://lite.duckduckgo.com/lite/) prevents routing ambiguities and ensures the lite interface remains available as a fallback path; it also guards against the historical issue where both endpoints pointed to the HTML host, which silently broke the lite layout.

---

## HtmlResultBlockRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlResultBlockRegex = new(
        @"<div[^>]*class=""[^""]*\bresult\b[^""]*""[^>]*>(?<body>.*?)(?=<div[^>]*class=""[^""]*\bresult\b|<div[^>]*id=""bottom_spacing""|\Z)",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


HtmlResultBlockRegex is a compiled, static readonly Regex used to extract individual result blocks from the HTML returned by the search endpoint. It locates a <div> with a class containing the word result and captures its inner content as body until the next result block, the bottom spacer, or end of input, enabling callers to iteratively process each result.

## Remarks
This abstraction centralizes the parsing logic around the server-generated markup, ensuring a single, well-defined way to pull out each result's body. Because the Regex is compiled and reused, it minimizes allocations and improves performance in hot paths that scan many results. The approach also makes it easy to revert to a fallback path (e.g., a lite-mode parser) if the upstream markup changes and the current pattern no longer matches expected blocks.

## Notes
- The pattern relies on server-side HTML structure; any change to the markup may require updating the regex and could break extraction.
- Due to being private and compiled, changes require code changes and recompilation; ensure tests cover both the pattern and its fallback behavior.

---

## HtmlSnippetRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlSnippetRegex = new(
        @"class=""[^""]*\bresult__snippet\b[^""]*""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


This private static readonly Regex centralizes the extraction of the visible snippet text from DuckDuckGo search result HTML. It matches an anchor tag whose class attribute includes result__snippet and captures the inner text in a capture group named text. The Regex is compiled and cached as a static field for performance, and Singleline is used so multiline snippet content is captured correctly.

## Remarks
By encapsulating this pattern as a single shared, compiled Regex, the class avoids duplicating the parsing logic for each result and ensures consistent extraction of snippet text across invocations. The field being private static readonly guarantees thread-safe reuse after initialization and prevents accidental reassignment.

## Notes
- HTML structure changes can break the regex-based parsing; prefer updating the pattern in one place.
- HtmlSnippetRegex is private; use public helpers in the class to access snippet text safely.
- The capture group name 'text' is part of the contract; renaming would break downstream usage.

---

## HtmlTitleLinkRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlTitleLinkRegex = new(
        @"<a[^>]*class=""[^""]*\bresult__a\b[^""]*""[^>]*href=""(?<href>[^""]+)""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


HtmlTitleLinkRegex is a private, precompiled Regex that targets the HTML anchor element used for a DuckDuckGo search result title. It captures two pieces of data from the matching anchor: the href attribute (named group href) and the displayed text inside the anchor (named group text). The field is stored as a static readonly instance to be reused across invocations, avoiding repetitive Regex construction and improving performance when parsing multiple search results.

## Remarks
Because the Regex instance is static and readonly, it is safe to reuse concurrently across threads. The Regex is compiled for speed and uses Singleline to tolerate HTML where the title may span line breaks. If the underlying HTML structure changes (for example, a different class name or tag shape for the result title link), this pattern would need updating to restore correct matching.

## Notes
- The regex returns only the first matching anchor; enumerate all occurrences with Regex.Matches if needed.
- The captured title text may contain HTML entities; decode them if the raw text is used directly.


---

## HttpClientName
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
public const string HttpClientName = "DuckDuckGoSearch"
```


Defines the literal name used to create or retrieve the HttpClient instance configured for the DuckDuckGo web search integration. Centralizing the client name avoids hard-coded strings scattered across the codebase and ensures consistent registration and retrieval via IHttpClientFactory.

Developers reach for this constant when they need a named HttpClient (e.g., when registering the client or obtaining it from DI) instead of constructing HttpClient directly.

## Remarks

This constant acts as an infrastructural glue between the DuckDuckGo web search component and its HTTP configuration. By naming the client, the app can apply per-client settings—such as base address, default headers, or timeouts—in one place, then reuse the same configured client wherever HTTP calls are made. Keeping the name in a single symbol prevents subtle bugs from string mismatches and makes the intent explicit across registration and usage sites.

## Notes

- Do not change HttpClientName's value without ensuring every registration and consumer that relies on the named client is updated.
- Ensure that there is a corresponding AddHttpClient(HttpClientName, ...) registration during startup; otherwise DI resolution may fail.

---

## LiteEndpoint
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string LiteEndpoint = "https://lite.duckduckgo.com/lite/"
```


Defines the base URL for the DuckDuckGo Lite search endpoint used by the web search logic. It is private and const, meaning the value is baked into the assembly and shared across the class's request construction, ensuring all internal calls consistently target the same endpoint.

## Remarks
Centralizes the lite endpoint to a single source of truth within the DuckDuckGo integration. Keeping it private signals that consumers should not rely on or modify this value directly; it can help with readability and future refactoring by isolating the endpoint in one place. If the Lite URL needs to change by environment, consider making the endpoint configurable rather than hard-coded.

## Notes
- Private const means the value is not accessible to external code and is compiled into the assembly.
- As a const field, it cannot be changed at runtime; modifying the endpoint would require recompiling the project.

---

## LiteResultLinkRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex LiteResultLinkRegex = new(
        @"<a[^>]*href=""(?<href>[^""]+)""[^>]*class='result-link'[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


This private, precompiled Regex is used by the lite endpoint parser to extract the destination URL and the visible title from anchor tags representing search results. It targets anchor elements with class='result-link', capturing the href value and the inner text as named groups href and text. The pattern aligns with the lite endpoint's HTML shape, and the surrounding parsing logic routes these anchors through UnwrapRedirect when necessary (the helper is a no-op if the redirect marker isn't present). The field is static readonly to avoid repeated compilation and to share the same instance across parsing operations.

## Remarks

By centralizing the parsing pattern, this symbol provides a single, maintainable point of change for how lite results are converted from raw HTML into structured data. Compiling the regex at type initialization improves performance under high-load scenarios where many results are parsed. Because the field is private, its usage is confined to the hosting class, reducing risk of misuse and keeping the HTML parsing concerns encapsulated. The named capture groups (href and text) enable downstream code to access the target URL and display text directly without additional parsing.

## Notes

- The regex assumes a specific HTML shape: a link with class='result-link' and an href attribute using double quotes, with the class attribute using single quotes. If the markup changes, this pattern may need updating.
- It uses named capture groups href and text to feed downstream processing without additional parsing.
- The RegexOptions.Singleline and RegexOptions.Compiled options optimize matching across multi-line anchors and improve startup performance, respectively.

---

## LiteSnippetRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex LiteSnippetRegex = new(
        @"<td[^>]*class='result-snippet'[^>]*>(?<text>.*?)</td>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


LiteSnippetRegex is a private, shared regular expression used to pull the visible snippet text from the HTML of a search results response. It targets a table cell element with class='result-snippet' and captures the inner content into a named group called "text". The pattern is initialized as a static readonly field and compiled for performance; the Singleline option allows the dot to match newline characters, enabling reliable extraction of multi-line snippets from results pages.

---

## TagStripRegex
> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex TagStripRegex = new(@"<[^>]+>", RegexOptions.Compiled)
```


TagStripRegex is a precompiled, shared Regex instance used to strip HTML-like tags from text. A developer would reach for it when needing a fast, centralized mechanism to remove tag markup (typically via Replace(input, "")) in content sourced from HTML responses, rather than building a new Regex each time.

## Remarks
Consolidating tag stripping into a static, readonly Regex ensures a single compiled instance is reused, reducing allocations and improving performance in text-cleaning code paths. The <[^>]+> pattern targets basic tags but is not a comprehensive HTML sanitizer; complex constructs like comments or scripts may require extra handling or a proper parser. Being private to the class, the field encapsulates the logic and promotes consistent, thread-safe tag removal across all call sites within the class.

## Notes
- The pattern is simplistic and may not cover all HTML edge cases (e.g., comments, CDATA, or script content) and could affect non-tag text in unusual input.
- The field is private; external code cannot reuse it directly—consider exposing a helper method if tag stripping should be shared.
- RegexOptions.Compiled improves performance on hot paths but can increase startup time and memory usage; ensure it aligns with your deployment constraints.

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


A private static readonly array named UserAgents stores a small pool of recent real-browser User-Agent strings. It is not rotated per request; instead, one UA is selected during session warmup and used for the entire session, mirroring how real browsers behave and avoiding rapid UA switching that can signal automation. The pool is designed to spread fingerprints across deployments and restarts rather than per-request changes, providing stable headers while still offering environmental variety.

## Remarks
This field acts as the centralized, session-scoped UA fingerprint source for HTTP headers used by the DuckDuckGo web search integration. By fixing a UA for the session, it reduces noise in fingerprints and lowers detection risk, while the pool itself allows re-seeding fingerprints across restarts. Because the array is private static readonly, the values can't be changed at runtime; updating the pool requires a code change and redeploy.

## Notes
- Changing the pool requires a rebuild and redeploy; the values are baked into the binary due to static initialization.

---
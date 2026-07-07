# Web search integration and HTTP utilities

> Web search integrations and underlying HTTP fetch utilities used by tools.

Web search integrations and HTTP fetch utilities provide two alternate IWebSearch implementations (one HTML-scraping, one API-based) plus a defensive URL fetcher that converts fetched pages into safe, size-bounded plain text. These components are intended for agent tooling and lightweight search scenarios: pick the Brave-backed implementation when you have an API key and need structured results, use the DuckDuckGo scraper for keyless, out-of-the-box searches, and use the URL fetcher when you need cleaned page text with SSRF protections for downstream consumers.

## DuckDuckGoWebSearch.cs
Implements web search via DuckDuckGo HTML endpoint.

The [DuckDuckGoWebSearch](../Code/src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs.md) class is a zero-configuration IWebSearch that scrapes DuckDuckGo's public HTML endpoints, preferring the richer html/ output and falling back to lite/ when needed. Concretely it exposes SearchAsync and a small suite of helpers: EnsureSessionAsync to pre-warm a cookie-backed session (using a SemaphoreSlim to dedupe concurrent warmups), FetchAsync and BuildRequest to perform HTTP fetches, and parsing routines ParseHtmlEndpoint and ParseLiteEndpoint driven by a set of regexes (HtmlResultBlockRegex, HtmlTitleLinkRegex, HtmlSnippetRegex, LiteResultLinkRegex, LiteSnippetRegex) and cleanup helpers like CleanText and FirstChars. The implementation also includes DetectAnomalyPage and UnwrapRedirect to detect rate-limit or anomaly pages and to follow or normalize redirects, and it defines constants for Homepage, HtmlEndpoint, LiteEndpoint, and an HttpClientName so consumers can configure the named HttpClient if they need custom handlers or timeouts.

## BraveWebSearch.cs
Implements Brave web search integration.

The [BraveWebSearch](../Code/src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs.md) class implements IWebSearch by calling Brave's /search HTTP API via a dependency-injected HttpClient that must be registered under the BraveSearch name. It maps the Brave API payload into the domain WebSearchResult items, clamps the requested result count into the 1–10 range, and centralizes error handling: it throws an InvalidOperationException when BraveSearchOptions.IsConfigured is false (guiding callers to provide an API key), and surfaces HTTP errors as exceptions. The mapping layer also normalizes optional fields to safe default strings so consumers of the returned IReadOnlyList<WebSearchResult> see a stable shape.

## HttpUrlFetcher.cs
Fetches raw HTML via HTTP for web tools.

The [HttpUrlFetcher](../Code/src/api/Gabriel.Infrastructure/Tools/Web/HttpUrlFetcher.cs.md) implements IUrlFetcher as a defensive wrapper around HttpClient designed for agents that feed web content to language models. It requires an IHttpClientFactory and uses the named client constant HttpUrlFetcher.HttpClientName ("WebFetch") when creating the HttpClient, so DI configuration of that named client controls timeouts/handlers/proxies. The fetcher enforces safety and size limits: it rejects non-absolute or non-http(s) URLs, blocks hosts that resolve to loopback/link-local/private addresses (SSRF protection), clamps the number of bytes read from the network and the number of characters returned, and refuses non-text Content-Types. It converts HTML into readable text by removing scripts/styles/nav/header/footer, stripping tags, decoding entities, collapsing whitespace, and sets UrlFetchResult.Truncated plus a "…[truncated]" marker when the result is cut short; it throws ArgumentException, HttpRequestException, or InvalidOperationException for the documented error cases.

How the pieces fit

These three files form two complementary layers: search providers and a safe fetcher. Both [DuckDuckGoWebSearch](../Code/src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs.md) and [BraveWebSearch](../Code/src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs.md) implement the same IWebSearch surface but differ in transport and reliability—DuckDuckGo scrapes HTML endpoints and contains parsing, session warmup, and fallback logic, while Brave delegates to a configured named HttpClient and the Brave API, requiring explicit configuration. Separately, [HttpUrlFetcher](../Code/src/api/Gabriel.Infrastructure/Tools/Web/HttpUrlFetcher.cs.md) provides a hardened IUrlFetcher for fetching and sanitizing page text; all three rely on DI-registered HttpClient instances (each exposes a named client constant) so application configuration controls timeouts, handlers, and proxies without leaking those details into callers.

---
*Synthesised by Aurion on 2026-07-07 18:11:47 UTC*

# Web search and fetch tooling

> Web search providers and fetch utilities used by the engine tooling.

# Web search and fetch tooling

This topic covers the small set of web-search provider implementations and the interface they implement. Read this when you need to understand how the system performs searches (either via a provider API or by scraping) and what guarantees callers may rely on about result shape, cancellation, and error behavior.

## DuckDuckGoWebSearch

Implements web search against DuckDuckGo endpoint.

The [DuckDuckGoWebSearch](../Code/src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs.md) class is a lightweight, no-API-key implementation of [IWebSearch](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) that scrapes DuckDuckGo’s public HTML endpoints. It prefers the richer html.duckduckgo.com/html/ endpoint and will fall back to lite.duckduckgo.com/lite/ if the HTML parsing yields no results; the file lists concrete helpers and steps such as BuildRequest, FetchAsync, ParseHtmlEndpoint, ParseLiteEndpoint, CleanText, DetectAnomalyPage, UnwrapRedirect, EnsureSessionAsync and ResetSession that implement the fetch/parse and session-warmup flow. Operational choices are explicit in the implementation: a single realistic User-Agent is chosen for a session, the session is pre-warmed by visiting the DuckDuckGo homepage so cookies are populated, concurrent warmups are deduplicated with a semaphore, parsing uses forgiving regexes (parse failures produce zero results rather than exceptions), and anomaly/anti-bot pages are logged in detail to aid diagnostics. Because it implements [IWebSearch](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md), callers receive a read-only collection of [WebSearchResult](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) and should expect HTML-driven limitations such as occasional rate-limiting behavior and less deterministic success compared to a paid API.

## BraveWebSearch

Implements Brave Web search provider.

The [BraveWebSearch](../Code/src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs.md) class is a concrete [IWebSearch](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) that calls the Brave Search API via an HttpClient. It validates configuration (and throws with clear guidance if the Brave API key is missing), clamps the requested result count to the 1–10 range, constructs the provider-specific request URL, executes the HTTP call, and maps Brave’s response body into the provider-agnostic [WebSearchResult](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) shape (Title, Url, Snippet). The implementation wires HttpClientFactory, BraveSearchOptions, and ILogger so concerns like authentication, timeouts, and error reporting are centralized; non-success HTTP responses are logged and rethrown as HttpRequestException containing the status code, making failures observable to callers.

## IWebSearch

`IWebSearch` collaborates directly with `BraveWebSearch` and other members of this topic (4 dependency links).

The [IWebSearch](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) interface defines the contract that both provider implementations satisfy: an asynchronous SearchAsync method that accepts a query, a limit, and a CancellationToken and returns a Task<IReadOnlyList<WebSearchResult>>. The file also declares the immutable [WebSearchResult](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) record (Title, Url, Snippet) that consumers use to work with results in a stable, provider-agnostic form. Implementations are required to honor the CancellationToken, observe the limit (returning at most that many items), and return a read-only, ordered collection that callers may rely on as an upper bound on length.

How the pieces fit

[IWebSearch](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) is the boundary between application logic and concrete search backends: both [BraveWebSearch](../Code/src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs.md) and [DuckDuckGoWebSearch](../Code/src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs.md) implement that contract but differ in trade-offs. BraveWebSearch calls a paid provider API and enforces configuration and HTTP error semantics (throwing on non-success responses), while DuckDuckGoWebSearch scrapes DuckDuckGo’s HTML endpoints, pre-warms sessions, and tolerantly parses HTML (returning zero results on parse failures and logging anomalies). Callers can swap providers without changing consumer code, rely on the [WebSearchResult](../Code/src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs.md) shape, and depend on cancellation and the documented limit behavior.

---
*Covers 3 of 3 source files identified for this topic.*

*Synthesised by Aurion on 2026-07-08 05:46:21 UTC*

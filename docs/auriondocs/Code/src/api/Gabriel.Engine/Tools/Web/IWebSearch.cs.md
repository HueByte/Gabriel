# IWebSearch.cs

> **Source:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`

## Contents

- [IWebSearch](#iwebsearch)
- [WebSearchResult](#websearchresult)

---

## IWebSearch
> **File:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`  
> **Kind:** interface

```csharp
public interface IWebSearch
```


IWebSearch defines an asynchronous abstraction over a web search provider and returns a read-only list of WebSearchResult for a given query. Use this interface when you want to query the web behind the scenes while keeping the provider pluggable—swap Brave, Tavily, SerpAPI, or any other implementation in Infrastructure without touching the agent layer, rather than calling a concrete HTTP client directly.

## Remarks
This abstraction isolates the agent-facing code from provider-specific HTTP details and response mapping, enabling you to swap in any compliant search service by changing only the Infrastructure implementation. WebSearchResult is the shared data shape that flows from the provider into the rest of the system, promoting a consistent consumption surface across different providers.

## Notes
- Propagate cancellation: pass the CancellationToken through to the underlying network calls and cancel promptly when ct is canceled.
- Respect the limit: return at most `limit` results; if the provider yields more, trim to the requested count.
- Avoid blocking: implementors should use true asynchronous I/O and handle transient network failures with appropriate error handling.

---

## WebSearchResult
> **File:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`  
> **Kind:** record

```csharp
public sealed record WebSearchResult(string Title, string Url, string Snippet)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Title` | `string` | — |
| `Url` | `string` | — |
| `Snippet` | `string` | — |


WebSearchResult is an immutable value object that represents a single entry returned by a web search. It aggregates a Title, a navigable Url, and a contextual Snippet, making it a convenient, transportable payload for UI rendering, API responses, or caching.

## Remarks
WebSearchResult provides a stable, minimal contract between a search provider and its consumers. By being a sealed record with value-based equality, it supports reliable comparisons, deconstruction, and safe sharing across layers without mutating state. The simple shape also makes it easy to serialize to JSON for API responses or cache stores, while preserving the original data.

## Notes
- This record is immutable; use with-expressions to create modified copies if needed.
- Url is a string; validate or convert to a Uri in consuming code if you need strict URL semantics.
- Snippet may contain HTML or formatting; escape accordingly when rendering to UI to avoid injection or misformatting.

---
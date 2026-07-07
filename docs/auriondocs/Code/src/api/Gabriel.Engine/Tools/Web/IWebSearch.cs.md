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


IWebSearch defines an asynchronous contract for a web search service, abstracting away the concrete provider from the rest of the system. It allows the agent layer to request search results without depending on a particular provider, enabling swapping implementations in Infrastructure without touching higher-level code.

## Remarks
IWebSearch serves as a crucial decoupling boundary between infrastructure and application logic. By depending on this interface, the system can experiment with Brave, Tavily, SerpAPI, or any future provider while keeping the agent and business logic stable. It also supports testing by letting developers provide lightweight mock implementations.

## Example
```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class EmptyWebSearch : IWebSearch
{
    public Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        // No external calls in this mock; return an empty result set.
        return Task.FromResult<IReadOnlyList<WebSearchResult>>(new List<WebSearchResult>());
    }
}
```

## Notes
- Observe the CancellationToken ct and propagate it to any awaited I/O operations to support cooperative cancellation.
- Implementations should perform real asynchronous I/O rather than blocking threads, and avoid throwing unless truly exceptional conditions occur.

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


WebSearchResult is a lightweight, immutable value object that models a single web search result, bundling its Title, Url, and Snippet for easy display and comparison. It is designed for transport and aggregation of search results within Gabriel.Engine and related tooling, providing a strongly-typed container instead of ad-hoc dictionaries or tuples.

## Remarks
Being a sealed record ensures value-based equality and prevents inheritance, making it safe to use as keys in dictionaries or in equality-checked collections. It also serves as a clear contract for search result data, reducing the surface area of misinterpretation when results flow through the system.

## Example
```csharp
var result = new WebSearchResult(
    Title: "OpenAI",
    Url: "https://openai.com",
    Snippet: "OpenAI develops AI models and tooling."
);
```

## Notes
- URLs are not validated or normalized by this type; downstream code should enforce URL validity as needed.

---
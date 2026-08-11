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


IWebSearch is an abstraction over a web search provider used by the WebSearchTool. Implementations supply SearchAsync to query a backend (Brave, Tavily, SerpAPI, etc.) and return up to a specified number of results as WebSearchResult; the interface enables swapping backends without modifying consumer code, while callers await the async results and may cancel via the CancellationToken.

## Remarks
WebSearch acts as the boundary between the application logic and concrete search backends. It decouples the consumer from a specific provider, enabling easy swapping (Brave, Tavily, SerpAPI) without touching higher-level code, and centralizes asynchronous search semantics via SearchAsync. By returning a Task<`IReadOnlyList<WebSearchResult>`> and accepting a CancellationToken, the contract favors asynchronous, cancelable queries and safe, read-only consumption of results. Implementations should honor the limit parameter and return at most that many items; consumers may assume the collection is immutable.

## Notes
- Implementations must observe and respond to the provided CancellationToken, canceling in a timely fashion when requested.
- The returned collection is `IReadOnlyList<WebSearchResult>`; callers should not attempt to mutate it, and may rely on its order and bounded length as an upper bound by the limit parameter.


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


WebSearchResult is an immutable value object that represents a single web search result, bundling the Title, Url, and Snippet together. Developers reach for this type when a web search API returns results that must be passed around, compared, or serialized as a single unit rather than handling separate strings or ad-hoc collections.

## Remarks

WebSearchResult is declared as a sealed record, which provides value-based equality and immutability out of the box. This makes it ideal as a boundary object between the web search service and its consumers, ensuring that two results with the same Title, Url, and Snippet compare as equal and that instances are inherently thread-safe due to immutability. By encapsulating the three related pieces of data in one type, the API gains a clear, discoverable shape for search results and easier serialization to JSON or other formats.

---
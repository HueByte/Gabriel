# IWebSearch.cs

> **Source:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`

## Contents

- [IWebSearch](#iwebsearch)
- [WebSearchResult](#websearchresult)

---

## IWebSearch

> **File:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`  
> **Kind:** interface

A simple abstraction for performing web searches against an external provider (for example: Brave, Tavily, SerpAPI). Use this interface from higher-level agent code when you need search results without depending on a concrete provider implementation; concrete implementations live in the Infrastructure layer and can be swapped without changing agent logic.

## Remarks
This interface decouples the agent/tool layer from specific web-search services. WebSearchTool and other consumers depend only on IWebSearch so different provider integrations can be implemented and registered via dependency injection in the infrastructure layer. Results are returned as a collection of WebSearchResult values.

## Example
```csharp
// Typical usage inside an agent/tool
public class MySearchAction
{
    private readonly IWebSearch _webSearch;

    public MySearchAction(IWebSearch webSearch)
    {
        _webSearch = webSearch;
    }

    public async Task DoSearchAsync(CancellationToken ct)
    {
        var results = await _webSearch.SearchAsync("latest C# features", limit: 5, ct);
        foreach (var r in results)
        {
            Console.WriteLine($"{r.Title}: {r.Url}");
        }
    }
}
```

## Notes
- Implementations should observe the provided CancellationToken and propagate cancellation (e.g., by throwing OperationCanceledException) when requested.
- The limit parameter is a maximum cap for returned results; implementations may return fewer results if the provider yields fewer matches.
- Network or provider errors are the responsibility of the implementation to surface (typically as exceptions); callers should handle transient failures and retries as appropriate. Prefer returning an empty IReadOnlyList over null to simplify callers.

---

## WebSearchResult

> **File:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`  
> **Kind:** record

A lightweight, immutable data container that represents a single web search hit: a display title, the result URL, and a short snippet or preview. Use this record when returning or transporting search results between a web search implementation (for example an IWebSearch) and UI or higher-level logic that only needs these basic fields.

## Remarks
This is a positional sealed record intentionally kept minimal so different search provider implementations can return a common shape without exposing provider-specific details. Being a record, it provides value-based equality, a synthesized Deconstruct method, and a helpful ToString implementation — useful for logging, testing, and comparisons.

## Example
```csharp
// Create a result
var result = new WebSearchResult("Example Title", "https://example.com", "A short snippet about the page.");

// Deconstruct
var (title, url, snippet) = result;

// Pattern matching
if (result is WebSearchResult { Url: var link })
{
    Console.WriteLine(link);
}

// Equality (value-based)
var copy = new WebSearchResult("Example Title", "https://example.com", "A short snippet about the page.");
Console.WriteLine(result == copy); // True
```

## Notes
- The record does not validate or normalize the Url; callers should validate/encode if required by consumers.
- Title and Snippet are plain strings; if HTML or unsafe content may be present they should be sanitized before display.
- The record is sealed and immutable (positional properties are init-only), so create a new instance to represent changes.

---
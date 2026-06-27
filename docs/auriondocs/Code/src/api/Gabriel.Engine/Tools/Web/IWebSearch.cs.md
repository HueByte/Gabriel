# IWebSearch.cs

> **Source:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`

## Contents

- [IWebSearch](#iwebsearch)
- [WebSearchResult](#websearchresult)

---

## IWebSearch

> **File:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`  
> **Kind:** interface

An abstraction for performing web searches against an external provider (Brave, Tavily, SerpAPI, etc.). Use this interface when agent or tool code needs search results but must remain independent of a specific provider implementation — concrete implementations live in infrastructure and can be swapped or mocked without changing the agent layer.

## Remarks
This interface intentionally keeps the contract minimal: callers request up to a given limit of results for a text query and supply a CancellationToken to allow cooperative cancellation. It exists to decouple higher-level tools (for example, WebSearchTool) from provider-specific details and to make testing and dependency injection straightforward.

## Example
```csharp
// Consumer code (e.g. inside an agent tool)
async Task ProcessSearch(IWebSearch webSearch, CancellationToken ct)
{
    var results = await webSearch.SearchAsync("how to make sourdough", 10, ct);
    foreach (var r in results)
    {
        Console.WriteLine($"{r.Title} - {r.Url}");
    }
}
```

## Notes
- Implementations perform network I/O and may throw network- or provider-related exceptions; callers should handle or surface those appropriately.
- A provider may return fewer results than the requested limit; do not assume the returned list length equals limit.
- Respect the CancellationToken: long-running provider calls should observe ct to allow prompt cancellation.
- The interface does not validate arguments (e.g., null or empty query, non-positive limit); callers or implementations should enforce their preferred validation rules.

---

## WebSearchResult

> **File:** `src/api/Gabriel.Engine/Tools/Web/IWebSearch.cs`  
> **Kind:** record

Represents a single web search hit containing a title, the result URL, and a short snippet (preview) of the matched content. Use this lightweight, immutable DTO when producing, returning, or transporting search results from a web search provider or indexing component.

## Remarks
This is a positional C# record: the primary constructor creates three init-only properties (Title, Url, Snippet) and the record uses value-based equality (two instances with equal property values compare equal). The record is declared sealed, so it is intended as a final data carrier rather than a base type. It is convenient for serialization, pattern matching, and deconstruction.

## Example
```csharp
// Create a search result
var result = new WebSearchResult(
    Title: "C# Records — Value-based equality",
    Url: "https://example.com/csharp-records",
    Snippet: "Records provide concise syntax for immutable data and structural equality..."
);

// Deconstruct and use
var (title, url, snippet) = result;
Console.WriteLine(title);

// Value equality
var copy = new WebSearchResult("C# Records — Value-based equality", "https://example.com/csharp-records", "Records provide concise syntax for immutable data and structural equality...");
Console.WriteLine(result == copy); // True
```

## Notes
- The record does not validate or normalize the Url or other string values; perform validation if required by callers.
- Equality and hash code are computed from the three properties; mutable operations (if any via reflection) would affect equality semantics.
- Because the record is sealed, it cannot be inherited; use composition if extension is needed.

---
# TavilyWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class TavilyWebSearch : IWebSearch
```


TavilyWebSearch implements IWebSearch and delegates search to Tavily’s API by POSTing a small JSON payload with a per-request Bearer token. The API returns an already-ranked list of results (title, url, content, score), where content is a snippet-sized field suitable for model consumption; the results are mapped to WebSearchResult objects for consumption by agents.

## Remarks
TavilyWebSearch hides Tavily-specific HTTP details behind the IWebSearch interface, enabling swapping or testing alternate search providers without changing callers. It sends both the API key in the request body and a Bearer token in the Authorization header to support key rotation at runtime without restarting the application; by disabling raw content, images, and answers it preserves a compact payload and maintains per-result citation integrity for reasoning steps.

## Notes
- The SearchAsync method throws an InvalidOperationException if Tavily is not configured; ensure TavilySearchOptions.IsConfigured is true or configure Tools:Web:Tavily:ApiKey.
- MaxResults is clamped to the range 1–20, so requests with higher limits will still return at most 20 results.
- The Authorization header is set per request, allowing API key rotation without restarting the host and avoiding unintended header persistence.
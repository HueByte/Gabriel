# TavilyWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs`  
> **Kind:** class

A concrete IWebSearch implementation that calls the Tavily Search API to return a small, already-ranked list of web results tailored for LLM/agent use. Reach for this when you need short, snippet-sized search hits (title, url, content) that are pre-truncated for model consumption and include scoring/citation-ready results instead of fetching full pages.

## Remarks
TavilyWebSearch wraps the Tavily HTTP API and maps its response into WebSearchResult instances. It uses an IHttpClientFactory-created client named "TavilySearch" and attaches the API key both as a per-request Bearer token and in the request body so the client can tolerate runtime key rotation. The implementation clamps the requested result count to the API's supported range and requests only the compact fields (no raw page content or images) because the service returns model-friendly snippets; this keeps network usage and payload size small for agent workflows.

## Example
```csharp
// Assume 'search' is an instance of TavilyWebSearch resolved from DI.
var results = await search.SearchAsync("how to set up grpc in dotnet", 5, CancellationToken.None);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}\n{r.Snippet}\n");
}
```

## Notes
- The Tavily API key must be configured (TavilySearchOptions.IsConfigured); otherwise SearchAsync throws InvalidOperationException.
- The requested limit is clamped to the range [1, 20].
- Non-success HTTP responses cause a logged warning and an HttpRequestException to be thrown.
- The response payload may be null; in that case an empty result list is returned. Null title/url/content fields are normalized to empty strings.
- Configure an HttpClient named "TavilySearch" in DI so the implementation can create it via IHttpClientFactory.
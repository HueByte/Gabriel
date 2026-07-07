# TavilyWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class TavilyWebSearch : IWebSearch
```


TavilyWebSearch is an IWebSearch implementation backed by Tavily's Tavily Search API. It posts a compact JSON payload along with a per-request Bearer token to Tavily's search endpoint and returns a pre-ranked list of results, each containing a Title, Url, and Content that is trimmed for model consumption. The results are exposed as WebSearchResult records with Title, Url, and Snippet (sourced from Content), ready for presentation to users or as input to a reasoning pipeline. The limit parameter is clamped to the range 1–20 to prevent excessive payloads, and when the Tavily API key is not configured, the class immediately throws InvalidOperationException to fail fast with a clear guidance message. To accommodate key rotation without application restart, the request attaches a Bearer token per call, and the API key is also supplied in the request body; this dual approach supports Tavily's authentication expectations while maintaining resilience to rotation. If Tavily responds with an error, TavilyWebSearch logs a warning including the HTTP status and response body, then throws HttpRequestException to surface failure to callers.

## Remarks
TavilyWebSearch encapsulates the Tavily API integration behind the IWebSearch contract, enabling the rest of the system to consume web-search results without coupling to Tavily’s wire format or transport details. By performing per-request authentication and constraining result size, it isolates token lifecycle concerns and protects downstream components from API over-fetching while preserving the model-friendly snippet surface used downstream for reasoning and presentation.

## Notes
- If Tools:Web:Tavily:ApiKey is not configured, the implementation throws InvalidOperationException with guidance on how to configure the API key.
- Limit values greater than 20 are clamped to 20; values below 1 are clamped to 1.
- Errors from the Tavily API surface as HttpRequestException after a warning log; callers should handle retries or user-facing failures as appropriate.
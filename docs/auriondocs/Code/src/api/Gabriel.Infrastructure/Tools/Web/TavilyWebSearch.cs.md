# TavilyWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs`  
> **Kind:** class

```csharp
// IWebSearch backed by the Tavily Search API (tavily.com). POSTs a small JSON
// payload + Bearer token; the response carries an already-ranked list of
//
```


Implements IWebSearch using the Tavily Search API to return pre-ranked, snippet-sized search results that are optimized for LLM consumption. Reach for this when you want short, model-friendly web search hits (title, URL, snippet) delivered via Tavily rather than doing raw page fetching or running your own ranking/trimming.

## Remarks
TavilyWebSearch wraps Tavily's POST-based search endpoint and maps its results into the local WebSearchResult shape. It intentionally requests compact content (no raw pages, no images, no cooked "answer") so results remain citation-friendly for downstream reasoning steps. The implementation attaches the API key per-request (and also includes it in the JSON body) so the underlying named HttpClient can remain reusable if credentials rotate; configuration is supplied via TavilySearchOptions resolved from IOptions.

## Example
```csharp
// Register the named HttpClient (BaseAddress must point to Tavily's API) and options in Startup/Program:
services.AddHttpClient(TavilyWebSearch.HttpClientName, c =>
{
    c.BaseAddress = new Uri("https://api.tavily.com/");
});
services.Configure<TavilySearchOptions>(configuration.GetSection("Tools:Web:Tavily"));
services.AddTransient<IWebSearch, TavilyWebSearch>();

// Usage from a consumer class with DI:
public class Consumer
{
    private readonly IWebSearch _webSearch;
    public Consumer(IWebSearch webSearch) => _webSearch = webSearch;

    public async Task UseSearch(CancellationToken ct)
    {
        var results = await _webSearch.SearchAsync("openai gpt-4o release notes", limit: 5, ct);
        foreach (var r in results)
        {
            Console.WriteLine($"{r.Title} - {r.Url}\n{r.Snippet}\n");
        }
    }
}
```

## Notes
- If TavilySearchOptions.IsConfigured is false (no API key), SearchAsync throws InvalidOperationException.
- Max results are clamped to the range 1..20; values outside that range are adjusted.
- The named HttpClient must have a valid BaseAddress that makes the relative "search" path resolvable; otherwise requests will fail.
- On non-success HTTP responses the class logs a warning and throws an HttpRequestException after reading the response body for context.
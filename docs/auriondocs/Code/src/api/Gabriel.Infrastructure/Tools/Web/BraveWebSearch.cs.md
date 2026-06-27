Implements IWebSearch using the Brave Search API via a named HttpClient (HttpClientName = "BraveSearch"). Use this class when you need to perform web searches backed by Brave and want the HTTP client configuration (base address, timeout, and subscription key header) managed by dependency injection instead of issuing raw HTTP requests.

## Remarks
This class centralizes Brave-specific request/response handling: it formats the query into a GET to /search, enforces a 1–10 result limit, deserializes only the subset of the JSON response the application consumes, and maps results to WebSearchResult instances. The class expects a named HttpClient configured elsewhere (DependencyInjection.AddInfrastructure) so auth and HTTP policies are kept out of the implementation and can be adjusted centrally or swapped for testing.

## Example
```csharp
// Assume BraveWebSearch is registered and injected via DI
public class SearchService
{
    private readonly IWebSearch _webSearch;

    public SearchService(IWebSearch webSearch) => _webSearch = webSearch;

    public async Task UseSearch(CancellationToken ct)
    {
        var results = await _webSearch.SearchAsync("open source c# libraries", 5, ct);
        foreach (var r in results)
        {
            Console.WriteLine($"{r.Title} - {r.Url}\n{r.Snippet}\n");
        }
    }
}
```

## Notes
- The BraveSearchOptions.IsConfigured flag must be true; otherwise SearchAsync throws InvalidOperationException to indicate a missing API key.
- The requested limit is clamped to the range 1–10 by the implementation; passing values outside this range will be adjusted.
- The code expects a named HttpClient called "BraveSearch" to be configured with the Brave base address and the X-Subscription-Token header (and any timeout/retry policies). If the HTTP response is non-success the response body is logged and an HttpRequestException is thrown.
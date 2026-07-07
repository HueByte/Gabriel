# HttpUrlFetcher

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/HttpUrlFetcher.cs`  
> **Kind:** class

```csharp
public sealed class HttpUrlFetcher : IUrlFetcher
```


Fetches a remote URL into a cleaned, size-bounded plain-text result while protecting the host from SSRF-style requests. Use this implementation of IUrlFetcher when you need a safe, agent-friendly fetcher that (1) refuses non-http(s) schemes and hosts that resolve to loopback/link-local/private ranges, (2) bounds the number of bytes read from the wire and the number of characters returned, and (3) converts HTML into readable text by removing page chrome (scripts, styles, nav/header/footer), stripping tags, decoding entities, and collapsing whitespace. Truncation is explicit (the returned UrlFetchResult.Truncated flag will be set and the content will include a "…[truncated]" marker).

## Remarks
This class is a defensive wrapper around HttpClient intended for use by agents or services that feed web content into language models or other consumers that require compact, readable text. It implements IUrlFetcher and relies on an IHttpClientFactory; the named client constant HttpClientName ("WebFetch") is used when creating the HttpClient instance, so configure that client in DI if you need custom timeouts, handlers, or proxy settings. The fetcher validates host reachability (to block requests that resolve to local or private addresses), enforces a maximum byte read from the network and a maximum number of characters returned, and rejects non-text Content-Types.

## Example
```csharp
// Register the named client (example in ASP.NET Core DI)
services.AddHttpClient(HttpUrlFetcher.HttpClientName)
        .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));

// Resolve and use (pseudo-code)
var fetcher = new HttpUrlFetcher(httpFactory, logger);
var result = await fetcher.FetchAsync("https://example.com/article", CancellationToken.None);
if (result.Truncated)
{
    Console.WriteLine("Content was truncated; only the first portion was returned.");
}
Console.WriteLine($"Final URL: {result.FinalUrl}");
Console.WriteLine(result.Content);
```

## Notes
- The method throws ArgumentException for non-absolute URLs or for schemes other than http/https.
- Non-success HTTP status codes cause an HttpRequestException, and non-text Content-Types cause an InvalidOperationException.
- Truncation can occur for two reasons: the fetch reached the MaxBytesToRead limit while reading the response stream, or the cleaned text exceeded MaxContentChars. The UrlFetchResult.Truncated flag is set if either occurred; ContentLength is the length of the cleaned (possibly truncated) string.
- Character decoding uses the response Content-Type charset when present; on decoding errors it falls back to UTF-8.
- Because a named HttpClient is used (HttpUrlFetcher.HttpClientName), consumers should register/configure that named client if they need specific handlers, proxy configuration, or timeouts.
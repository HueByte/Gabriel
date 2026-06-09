# HttpUrlFetcher

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/HttpUrlFetcher.cs`  
> **Kind:** class

Fetches a remote HTTP(S) URL and returns a cleaned, text-first representation suitable for downstream consumption (for example, feeding into a model). Use this implementation when you need a safe, bounded web fetcher that defends against SSRF, enforces size limits, and extracts readable text from HTML pages instead of returning raw markup.

## Remarks
HttpUrlFetcher is an IUrlFetcher implementation designed for safe, predictable web retrieval inside server-side applications. It enforces three cross-cutting concerns: it rejects non-HTTP(S) URIs and hosts that appear to be internal/loopback/private (SSRF protection), bounds reads from the network (MaxBytesToRead) and truncates long extracted text (MaxContentChars), and converts HTML to plain text by removing noisy elements (scripts/styles/navigation/footer) then collapsing whitespace and decoding entities. The class uses an IHttpClientFactory-created HttpClient named "WebFetch" so the network behavior (timeouts, proxy, DNS policy) can be configured centrally. The returned UrlFetchResult contains the final request URL, content type, cleaned text, a Truncated flag, and the character length.

## Example
```csharp
// Register the named HttpClient during startup/configuration
services.AddHttpClient(HttpUrlFetcher.HttpClientName)
        // configure timeouts, handlers, etc. as appropriate
        .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));

// Resolve and use the fetcher (usually injected)
var fetcher = serviceProvider.GetRequiredService<IUrlFetcher>();
var result = await fetcher.FetchAsync("https://example.com/article", CancellationToken.None);
Console.WriteLine(result.Content);
if (result.Truncated) Console.WriteLine("(content was truncated)");
```

## Notes
- Throws ArgumentException if the URL is not an absolute http(s) URI.
- Throws HttpRequestException when the upstream response status is not successful (non-2xx).
- Throws InvalidOperationException when the response Content-Type is not considered text-like.
- Truncation is explicit: the returned UrlFetchResult.Truncated is true if the read from the wire hit MaxBytesToRead or if the cleaned text was trimmed to MaxContentChars (the code appends "\n…[truncated]" when character truncation occurs).
- Character encoding: the fetcher attempts to use the response charset; on failure it falls back to UTF-8.
- The final URL may differ from the requested URL due to redirects; FinalUrl is taken from the response.RequestMessage.RequestUri when available.
- The implementation relies on a named HttpClient (HttpUrlFetcher.HttpClientName); ensure that client is configured appropriately in your DI container for timeouts, proxies, and DNS/handler policies.
- CancellationToken is observed for the network operations where supported.

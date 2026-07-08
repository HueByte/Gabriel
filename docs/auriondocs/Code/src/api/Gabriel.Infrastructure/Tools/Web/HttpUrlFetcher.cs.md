# HttpUrlFetcher

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/HttpUrlFetcher.cs`  
> **Kind:** class

```csharp
public sealed class HttpUrlFetcher : IUrlFetcher
```


Fetches an HTTP(S) URL, returning a sanitized, size-capped text representation suitable for feeding into a model or other text-processing components. Use this implementation when you need a safe, predictable fetcher that defends against SSRF (rejecting non-public addresses and non-HTTP(S) schemes), enforces byte/character limits, and converts HTML pages into readable plain text rather than returning raw markup.

## Remarks
HttpUrlFetcher centralizes three practical concerns for web-to-text ingestion: network safety, bounded reads, and HTML-to-text conversion. It obtains HttpClient instances from an IHttpClientFactory (client name "WebFetch"), enforces that URLs are absolute http/https, and calls an internal host-check to refuse hostnames that resolve to loopback, link-local, or RFC1918 ranges. Responses are accepted only when the upstream status is successful and the Content-Type is text-like; binary content is refused.

To protect downstream consumers it performs a bounded read (MaxBytesToRead) from the response stream and then converts bytes to text using the declared charset with a UTF-8 fallback. HTML responses are cleaned (script/style/footer/nav removal, tag-stripping, entity decoding, whitespace collapse) and the final text is truncated at MaxContentChars with a visible "…[truncated]" marker. The returned UrlFetchResult contains the final URL, content type, cleaned content, a Truncated flag that indicates whether truncation occurred (either at the byte or character limit), and the cleaned content length.

## Example
```csharp
// Typical usage within an application (DI or manual construction):
IHttpClientFactory httpFactory = /* obtain from DI */;
ILogger<HttpUrlFetcher> logger = /* obtain logger */;
var fetcher = new HttpUrlFetcher(httpFactory, logger);

var result = await fetcher.FetchAsync("https://example.com/article", CancellationToken.None);
if (result.Truncated)
{
    // handle truncated content awareness
}
Console.WriteLine(result.Content);
```

## Notes
- The fetcher enforces SSRF protections: it requires absolute http/https URIs and will refuse hosts that resolve to local/private addresses (the internal AssertPublicHostAsync performs this check and may throw).
- Truncation is explicit: the Truncated flag is set if the byte-read cap or the character cap was hit, and character truncation appends "\n…[truncated]" to the returned Content.
- Charset handling uses the response's Content-Type charset when present; if decoding fails the implementation falls back to UTF-8, so callers should not assume the returned text exactly matches the original byte semantics for exotic encodings.
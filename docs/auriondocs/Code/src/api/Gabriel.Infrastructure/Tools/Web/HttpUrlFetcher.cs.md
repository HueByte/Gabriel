# HttpUrlFetcher

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/HttpUrlFetcher.cs`  
> **Kind:** class

```csharp
public sealed class HttpUrlFetcher : IUrlFetcher
```


Fetches an HTTP(S) URL and returns a cleaned, text-first representation suitable for feeding into downstream models. Use this when you need a web fetcher that defends against SSRF, bounds response size, converts HTML to readable text (stripping scripts/styles/navigation/footer and collapsing whitespace), and reports truncation instead of silently dropping content.

## Remarks
This implementation wraps IHttpClientFactory (it expects a named client with the constant name "WebFetch") and is intentionally conservative: it only allows http/https schemes, validates hosts to avoid loopback/link-local/RFC1918 addresses (to reduce SSRF risk), refuses non-text Content-Types, and bounds both the bytes read from the wire and the characters returned. HTML content is passed through an HTML-cleaning routine (removes chrome-like blocks, strips tags, decodes entities, collapses whitespace) so callers receive the readable body rather than full page markup.

## Notes
- Input validation: throws ArgumentException if the URL is not an absolute URI or if the scheme is not http/https.
- Network/result errors: non-success HTTP status codes surface as HttpRequestException; non-text Content-Type values cause an InvalidOperationException.
- Size limits: reads at most 1_500_000 bytes from the network (MaxBytesToRead) and truncates the returned text to 12_000 characters (MaxContentChars). Truncation is indicated in the UrlFetchResult.Truncated flag; character truncation appends "\n…[truncated]" to the content.
- Encoding: uses the response Content-Type charset when available; if decoding fails the code falls back to UTF-8.
- FinalUrl: the returned FinalUrl prefers response.RequestMessage?.RequestUri if available (followed redirects are reflected there), otherwise the original URI is used.
- Configuration: the class calls _httpFactory.CreateClient(HttpClientName) — the caller must register a named HttpClient with the name "WebFetch" if custom HTTP settings (timeouts, handlers, DNS policies) are required.
- Cancellation: the provided CancellationToken is passed through network and host-validation operations; callers should supply one to allow timely shutdown.


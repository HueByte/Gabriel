# HttpUrlFetcher

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/HttpUrlFetcher.cs`  
> **Kind:** class

Fetches a web URL and returns a sanitized, size-limited plain-text representation suitable for feeding into an LLM or other text-processing pipeline. Use this when you need a safe, predictable extraction of a page's readable content (HTML cleaned to text, entities decoded, whitespace collapsed) with built-in SSRF protections and explicit truncation reporting — not when you need binary downloads or the full raw HTML.

## Remarks
HttpUrlFetcher centralizes three concerns: (1) SSRF defense (it accepts only http/https and refuses hosts that resolve to loopback, link-local or private RFC1918 addresses), (2) size limiting (it bounds the bytes read from the wire and also caps the returned character count, marking results as truncated when either limit is reached), and (3) content normalization (HTML pages have script/style/nav/header/footer removed, remaining tags stripped, entities decoded and whitespace collapsed). The implementation uses an IHttpClientFactory-created client named "WebFetch", streams headers-first (ResponseHeadersRead) to avoid buffering large responses, and falls back to UTF-8 if the response charset is unknown or unsupported.

## Example
```csharp
// Typical usage when you have DI-provided IHttpClientFactory and ILogger<HttpUrlFetcher>:
var fetcher = new HttpUrlFetcher(httpFactory, logger);
var result = await fetcher.FetchAsync("https://example.com/article", CancellationToken.None);
if (result.Truncated)
{
    Console.WriteLine("Content was truncated; consider fetching the source or increasing limits.");
}
Console.WriteLine(result.Content);
```

## Notes
- The class expects an IHttpClientFactory and uses the named client "WebFetch"; ensure that client is registered/configured (timeouts, proxy, etc.) in your DI container.
- The method throws ArgumentException for non-absolute or non-http(s) URLs, HttpRequestException for non-success responses, and InvalidOperationException when the Content-Type is not text-like; callers should handle these cases.
- Charset decoding falls back to UTF-8 on errors; pages using uncommon legacy encodings may be mis-decoded and require special handling upstream.
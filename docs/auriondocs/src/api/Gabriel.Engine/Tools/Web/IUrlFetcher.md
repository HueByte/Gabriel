# IUrlFetcher.cs

> **Source:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`

## Contents

- [IUrlFetcher](#iurlfetcher)
- [UrlFetchResult](#urlfetchresult)

---

## IUrlFetcher

> **File:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`  
> **Kind:** interface

Fetches a public URL and returns a UrlFetchResult containing cleaned, agent-friendly plain text (and metadata). Reach for this interface when higher-level tools need normalized web content but you want network access, security checks (SSRF protections), size limits, and HTML-to-text conversion encapsulated and replaceable.

## Remarks
IUrlFetcher isolates URL fetching and sanitization concerns from agent logic. Implementations are expected to enforce HTTP(S)-only schemes, block private/loopback addresses to mitigate SSRF, cap response size so large pages don't exhaust model context, and convert HTML into plain text (removing scripts/styles/navigation before stripping tags). This abstraction lets callers remain agnostic about networking, security, and conversion details and enables swapping different fetcher strategies for testing or different runtime environments.

## Example
```csharp
// Caller code — implementation provided elsewhere
IUrlFetcher fetcher = /* resolve IUrlFetcher from DI or create implementation */;
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

UrlFetchResult result = await fetcher.FetchAsync("https://example.com/article", cts.Token);

Console.WriteLine("Final URL: " + result.FinalUrl);
Console.WriteLine("Content-Type: " + result.ContentType);
Console.WriteLine("Content length (cleaned): " + result.ContentLength);
if (result.Truncated)
{
    Console.WriteLine("Note: content was truncated to the configured size cap.");
}

// The cleaned, plain-text body suitable for the agent is in result.Content
Console.WriteLine(result.Content);
```

## Notes
- Implementations must reject non-HTTP(S) schemes and block private, link-local, and loopback addresses when resolving URLs to prevent SSRF.
- Enforce a configurable maximum response size and set UrlFetchResult.Truncated = true when content exceeds the cap; ContentLength measures the cleaned text length.
- Honor the CancellationToken and apply sensible timeouts; convert HTML to text by removing scripts/styles/navigation before stripping tags to avoid leaking irrelevant or unsafe text.

---

## UrlFetchResult

> **File:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`  
> **Kind:** record

Represents the result of fetching a web resource: the final resolved URL (after redirects), the response Content-Type, the cleaned textual content, a flag indicating whether the original page was truncated due to a size cap, and the length of the cleaned content. Use this record as the return value from an URL fetcher implementation or anywhere you need both the text content and simple metadata about the fetch.

## Remarks
This record is designed to carry sanitized text and minimal fetch metadata to downstream components (parsers, indexers, summarizers) without exposing raw response bytes or headers. "FinalUrl" records the URL after any redirects so callers can attribute or re-request the canonical location; "Truncated" signals that the content was cut to respect an internal cap; "ContentLength" reflects the length of the cleaned "Content" rather than the original response size.

## Example
```csharp
var result = new UrlFetchResult(
    FinalUrl: "https://example.com/article",
    ContentType: "text/html; charset=utf-8",
    Content: "This is the cleaned text of the page.",
    Truncated: false,
    ContentLength: 33);

Console.WriteLine($"Fetched {result.FinalUrl} (type={result.ContentType}), length={result.ContentLength}");
if (result.Truncated) Console.WriteLine("Note: content was truncated by the fetcher.");
```

## Notes
- ContentLength is intended to reflect the length of the cleaned Content; do not assume it represents raw byte size of the original response.
- Truncated == true only indicates the fetcher cut the page to enforce a cap; it does not provide the original full size.
- If you create a modified copy of the record (using with or by manual construction), ensure ContentLength matches the new Content if you rely on it elsewhere.

---
# IUrlFetcher.cs

> **Source:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`

## Contents

- [IUrlFetcher](#iurlfetcher)
- [UrlFetchResult](#urlfetchresult)

---

## IUrlFetcher

> **File:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`  
> **Kind:** interface

Fetches and sanitizes the contents of a public URL and returns a UrlFetchResult suitable for downstream agents. Use this interface whenever an agent or tool needs a safe, size-limited, plain-text representation of a remote page instead of raw HTTP responses.

## Remarks
This abstraction isolates fetching and HTML-to-text conversion from agent logic so different strategies (mocking, testing, stricter or laxer sanitization, platform-specific network policies) can be swapped without changing caller code. Implementations are expected to enforce security and size constraints (SSRF protection, scheme checks, response-size caps) and to convert HTML into cleaned plain text.

## Example
```csharp
// `fetcher` is an injected IUrlFetcher implementation
CancellationToken ct = CancellationToken.None;
var result = await fetcher.FetchAsync("https://example.com", ct);
// Inspect the returned UrlFetchResult to determine success and to consume the cleaned text
```

## Notes
- Implementations must reject non-HTTP/HTTPS schemes and guard against internal/private/loopback addresses (SSRF protection).
- Cap response size to avoid returning very large documents that could exhaust model context or memory.
- Convert HTML to plain text by removing script/style/navigation sections and then stripping remaining tags so the agent receives readable text.
- Observe the provided CancellationToken and fail fast when cancellation is requested.
- Be mindful of redirects: a fetch that starts on a public URL may be redirected to an internal address, so SSRF checks should be applied after following redirects.

---

## UrlFetchResult

> **File:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`  
> **Kind:** record

A compact container for the outcome of fetching a web resource. Use this when consuming an IUrlFetcher (or any URL-fetching abstraction) to get the normalized, cleaned text of a page along with basic metadata: the final resolved URL after redirects, the response content type, whether the fetch hit a configured size cap (Truncated), and the length of the cleaned content.

## Remarks
This record centralizes the post-fetch normalization that callers typically need: redirect resolution (FinalUrl), MIME information (ContentType), and a cleaned/plain-text representation of the payload (Content). The Truncated flag and ContentLength make it explicit when the returned text is only a prefix of the original page due to size limits; clients that need full HTML or binary data should use a different API.

## Example
```csharp
// Constructing a result manually (most callers will receive this from an IUrlFetcher):
var result = new UrlFetchResult(
    FinalUrl: "https://example.com/page",
    ContentType: "text/html; charset=utf-8",
    Content: "This is the cleaned text of the page.",
    Truncated: false,
    ContentLength: "This is the cleaned text of the page.".Length);

// Inspecting a returned result:
Console.WriteLine($"Fetched {result.FinalUrl} (type={result.ContentType})");
if (result.Truncated)
    Console.WriteLine("Note: content was truncated to a maximum allowed size.");

// Use result.Content for indexing, summarization, or plain-text analysis.
```

## Notes
- ContentLength is the length of the cleaned Content (i.e., Content.Length), not the original byte length of the HTTP response.
- Truncated = true means the original page exceeded an internal cap; do not assume the text represents the full page.
- Content is "cleaned" text (HTML removed/normalized) — it may have collapsed whitespace and character normalization applied, and may not be suitable when raw HTML is required.

---
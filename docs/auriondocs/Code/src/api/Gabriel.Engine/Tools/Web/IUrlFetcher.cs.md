# IUrlFetcher.cs

> **Source:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`

## Contents

- [IUrlFetcher](#iurlfetcher)
- [UrlFetchResult](#urlfetchresult)

---

## IUrlFetcher
> **File:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`  
> **Kind:** interface

```csharp
public interface IUrlFetcher
```


IUrlFetcher provides a contract for asynchronously fetching the contents of a public URL and returning a cleaned, plain-text representation suitable for consumption by an agent. Implementations are expected to enforce HTTP/HTTPS schemes, reject SSRF-prone addresses (internal/private/loopback), cap response size to protect model context, and convert HTML to text by stripping scripts, styles, navigation, and tags. Use this abstraction instead of performing raw HTTP calls directly when you need a consistent, sanitized input source that can be swapped out without affecting the agent layer (for testing, mocking, or alternate fetch backends).

## Remarks
Isolates network access and content sanitization behind a single abstraction, reducing coupling between the agent and transport concerns. The WebFetchTool relies on this interface, enabling swapping in different fetch implementations (for tests or different environments) without changing the agent code. It also codifies the sanitization policy at the boundary, ensuring uniform plain-text output for downstream processing.

## Example
```csharp
// Example usage with a cancellation token
IUrlFetcher fetcher = /* provided by DI or a test double */;
UrlFetchResult result = await fetcher.FetchAsync("https://example.org", CancellationToken.None);
```

## Notes
- UrlFetchResult is expected to contain cleaned text, not the original HTML; do not rely on the presence of raw markup in downstream usage.
- Respect the provided CancellationToken and honor timeouts; callers may cancel the operation to preserve responsiveness.

---

## UrlFetchResult
> **File:** `src/api/Gabriel.Engine/Tools/Web/IUrlFetcher.cs`  
> **Kind:** record

```csharp
public sealed record UrlFetchResult(
    string FinalUrl,         
    string ContentType,
    string Content,          
    bool Truncated,          
    int ContentLength)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `FinalUrl` | `string` | — |
| `ContentType` | `string` | — |
| `Content` | `string` | — |
| `Truncated` | `bool` | — |
| `ContentLength` | `int` | — |


UrlFetchResult is an immutable data carrier that encapsulates the essential outcome of a URL fetch. It records the final URL after redirects, the content type, the cleaned textual content, whether the content was truncated due to a cap, and the length of the cleaned content. This enables callers to reason about fetch results without depending on low-level HTTP details, and to present or analyse the page content in a stable, serializable form.

## Remarks

As a record, UrlFetchResult provides value-based equality and immutability, making it convenient for caching, deduplication, and passing fetch results across components. The five fields together separate transport concerns (FinalUrl, ContentType) from payload concerns (Content, Truncated, ContentLength), offering a clear contract for consumers that only need the meaningful outcome of a fetch.

## Example

```csharp
// Example usage: create a result and inspect its properties
var result = new UrlFetchResult(
    FinalUrl: "https://example.com/",
    ContentType: "text/html",
    Content: "Welcome to Example.com",
    Truncated: false,
    ContentLength: 22
);

Console.WriteLine($"Fetched: {result.FinalUrl}, Type: {result.ContentType}, Truncated: {result.Truncated}, Length: {result.ContentLength}");
```

## Notes

- UrlFetchResult is immutable; use a "with" expression to derive a modified copy if you need a variant.
- If Truncated is true, Content contains the cleaned portion only, and ContentLength reflects the length of Content, not the original page size.

---
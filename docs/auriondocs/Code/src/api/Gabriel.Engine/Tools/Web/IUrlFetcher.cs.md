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


IUrlFetcher defines an asynchronous contract for retrieving the contents of public URLs and returning a sanitized UrlFetchResult for downstream consumption by the agent layer. Implementations should enforce security constraints (allow only HTTP(S) schemes, guard against SSRF by avoiding private or loopback addresses), cap the response size to prevent context overrun, and transform HTML into plain text by stripping scripts, styles, navigation, and tags before returning.

## Remarks
To maintain a clean separation of concerns, this abstraction lets the agent layer remain agnostic to how content is fetched or cleaned. Different fetch strategies (for example, in-process mocks vs. real HTTP clients) can be swapped behind this interface without touching higher layers, and callers rely on UrlFetchResult to represent the outcome and content.

## Notes
- Validate URL schemes and address scope to prevent SSRF.
- Honor CancellationToken and avoid blocking indefinitely.
- Cap/trim response content to keep within model context, signaling truncation in UrlFetchResult if applicable.

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


UrlFetchResult is an immutable record that represents the outcome of fetching a URL. It exposes the final URL after redirects, the content type, the cleaned textual content, a flag indicating whether the content was truncated due to a cap, and the length of the cleaned content for downstream processing.

## Remarks
As a single value object, UrlFetchResult decouples the fetch implementation from its consumers. It provides both metadata (FinalUrl, ContentType) and content (Content) in a stable shape that downstream components such as parsers, indexers, or UIs can rely on. Being a record, its equality is value-based and instances are immutable, making it straightforward to cache or deduplicate results. FinalUrl helps you follow redirects, Truncated signals whether the original page was capped, ContentType informs parsing strategy, and ContentLength offers a quick size metric.

## Notes
- ContentLength reflects Content.Length after cleaning; if you swap the cleaning strategy, this value will change and should not be treated as the raw page length.

---
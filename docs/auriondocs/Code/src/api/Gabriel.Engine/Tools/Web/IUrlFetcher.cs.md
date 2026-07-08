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


IUrlFetcher defines an asynchronous contract for fetching public HTTP(S) URLs and returning cleaned, plain-text content suitable for agent consumption. Implementations must guard against SSRF by rejecting non-HTTP(S) schemes and internal/private/loopback addresses, cap response size to protect the model's context, and convert HTML into readable plain text by stripping scripts, styles, navigation elements, and all tags.

## Remarks
By isolating the fetch operation behind IUrlFetcher, the agent layer remains decoupled from transport and content-sanitization concerns. Different implementations (for example, in-process mocks for testing or production-grade fetchers using an HTTP client) can be swapped without altering callers, enabling centralized enforcement of URL validation and content normalization across the codebase.

## Notes
- Always propagate cancellation and respect timeouts; callers may cancel via the provided CancellationToken.
- Enforce strict content-size caps to prevent huge responses from inflating the model's context, and ensure HTML-to-text conversion strips scripts/styles and tag noise before returning UrlFetchResult.

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


UrlFetchResult is an immutable data carrier that encapsulates the outcome of fetching a URL. It records the final URL after redirects, the Content-Type reported by the server, and the cleaned textual Content prepared for downstream processing. It also indicates whether the retrieved content was truncated due to a configured cap and exposes ContentLength for the length of the cleaned Content.

## Remarks

UrlFetchResult is designed as a simple, transport-friendly representation of a fetch operation. FinalUrl reflects the actual destination after redirects, which is useful for logging, analytics, or follow-up requests. ContentType hints at how to interpret Content (for example, HTML vs. JSON), and Truncated communicates that the original payload exceeded the configured cap, so Content may be incomplete. ContentLength provides the size of Content after cleaning, which aids progress reporting and prevents unnecessary re-processing. Being a record, it supports deconstruction and pattern matching without mutating state.

## Example

```csharp
var result = new UrlFetchResult(
    FinalUrl: "https://example.org/",
    ContentType: "text/html",
    Content: "Cleaned text of the page",
    Truncated: false,
    ContentLength: 128
);

var (finalUrl, contentType, content, truncated, length) = result;
```

## Notes

- ContentLength is the length of Content after cleaning, not the original page size.
- Truncated being true means the original page exceeded the retrieval cap, so Content may be incomplete.
- FinalUrl may differ from the initially requested URL due to server redirects; use it for downstream requests and logging.

---
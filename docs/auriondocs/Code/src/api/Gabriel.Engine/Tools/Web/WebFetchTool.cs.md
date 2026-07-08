# WebFetchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs`  
> **Kind:** class

```csharp
public sealed class WebFetchTool : ITool
```


Fetch and read the actual content of a public web page by URL, returning cleaned plain text (HTML tags stripped, script/style/nav removed, whitespace normalized) capped at roughly 12,000 characters. Use this after web_search when a result snippet looks relevant and you need the full page text to answer the user; do not use this for Gabriel-specific questions—use docs_read for those.

## Remarks
WebFetchTool acts as a thin, pluggable adapter in the tool ecosystem, delegating the network fetch to an IUrlFetcher and presenting a consistent plain-text payload. By emitting a small header with the FinalUrl, Content-Type, and Length before the content, it makes it easier to diagnose fetch results and content characteristics without parsing the body. Keeping the fetch logic isolated from downstream reasoning helps ensure predictable behavior and easier testing.

## Notes
- Error handling is conservative: invalid or missing URL returns a string starting with 'Error:' rather than throwing.
- Output is plain text; HTML and site navigation are removed, but some semantic cues may be lost depending on parsing.
- If the underlying URL fetcher is not network-accessible or blocks requests, the tool will return an error string instead of raising.

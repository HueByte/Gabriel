# WebFetchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs`  
> **Kind:** class

```csharp
public sealed class WebFetchTool : ITool
```


WebFetchTool fetches and reads the full text content of a public web page by URL. Use this after web_search when a result snippet looks relevant and you need the full page text to answer the user—the snippets are short and often miss relevant detail. It returns cleaned plain text (HTML tags stripped, script/style/nav removed, whitespace normalized), capped at roughly 12,000 characters. Do not use this for Gabriel-specific questions—use docs_read for those.

## Remarks
WebFetchTool is a thin wrapper around an injected IUrlFetcher. It coordinates input validation, invocation, and presentation of results as a single, human-readable string. The output begins with an informational header including the final URL, content type, and content length, followed by the page content. This design keeps the fetch logic decoupled from how the results are consumed by downstream tooling and fits naturally into a web-search -> fetch workflow where you need the full page text for deeper analysis.

## Notes
- If the input JSON is missing the required "url" field, or the value is not a string or is empty, ExecuteAsync returns an error message instead of throwing.
- If the fetched page is larger than the internal limit, the result will include "(truncated - page was larger)" after the length to signal incomplete content.
- The first lines of the returned string reveal the final URL after any redirects, the content type, and the length, which aids debugging and provenance of the fetched content.
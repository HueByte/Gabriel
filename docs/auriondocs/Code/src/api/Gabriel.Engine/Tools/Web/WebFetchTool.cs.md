# WebFetchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs`  
> **Kind:** class

```csharp
public sealed class WebFetchTool : ITool
```


WebFetchTool is a sealed ITool that fetches and reads the full content of a public web page from a URL. It delegates the HTTP request to an injected IUrlFetcher and returns a single string containing lightweight metadata plus the cleaned page text. This is intended to be used after a web_search step identifies a potentially relevant result, since search snippets often miss important details. The returned content is sanitized (HTML tags stripped, scripts/styles/nav removed, whitespace normalized) and capped at roughly 12,000 characters. If the URL is invalid or the fetch fails, the method returns an error message rather than throwing.

## Remarks
WebFetchTool isolates the page-fetching concern from query discovery, enabling swap-in of different fetchers or mock implementations for tests. The header lines ('Fetched', 'Content-Type', 'Length') provide quick auditing of the resolved URL and payload without requiring separate parsing of the content. This abstraction focuses on turning a URL into a plain-text representation that downstream components can analyze or present to users.

## Notes
- The ExecuteAsync method returns strings starting with "Error:" for invalid input or fetch failures instead of throwing exceptions.
- The content is capped around 12,000 characters; large pages will be truncated, with a flag in metadata indicating truncation.
- The URL must be absolute (http or https). The tool does not perform or retry in-domain redirects beyond what the fetcher reports.
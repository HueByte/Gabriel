ParseLiteEndpoint is a private static helper that converts a chunk of lite DuckDuckGo HTML into a List<WebSearchResult>. It scans the HTML for result links and, in parallel, for their corresponding snippets. For each link up to the provided limit, it HTML-decodes the href, cleans the visible text to produce a title, unwraps possible redirects to yield a stable URL, and attaches a snippet if one exists. Entries with missing title or URL are discarded. Snippets are matched by index to their links, and if no snippet is available for a given entry, the snippet is left empty. The resulting collection is ordered to reflect the original document order and can be used by higher-level search UI or processing code.

## Remarks
This method encapsulates the brittle HTML parsing required to extract meaningful search results from the lite DuckDuckGo layout, exposing a strongly-typed collection to downstream code. By decoupling the parsing logic from presentation, it keeps the rest of the pipeline focused on rendering and interaction. The snippet alignment strategy is simple and relies on the document order; as long as the lite layout remains stable, the produced WebSearchResult items stay synchronized with their corresponding snippets. The URLs are normalized via UnwrapRedirect to ensure consumers receive clean, direct links.

## Example
```csharp
// Example inside the same class that contains ParseLiteEndpoint
string html = GetLiteDuckDuckGoResultHtml();
List<WebSearchResult> results = ParseLiteEndpoint(html, 10);
// results contains up to 10 items with Title, Url, and optional Snippet
```

## Notes
- The method is private; to expose this functionality publicly, wrap it in another method or elevate access accordingly.
- If the HTML structure changes, the LiteResultLinkRegex or LiteSnippetRegex may fail to match, which would reduce or alter the results returned.
- If a link has no title or a blank URL, it is skipped; the snippet for that entry, if any, is ignored since the entry itself is discarded.

Parses HTML produced by DuckDuckGo's "lite" search layout and returns up to `limit` WebSearchResult entries. Use this when you have raw HTML from the lite endpoint and need a small, ordered list of results (title, target URL, and an optional snippet) rather than scraping the full page or using an API.

## Remarks
This method uses two regular expressions (one for result links, one for snippets) to extract result elements and pairs them by document order: the i-th link is associated with the i-th snippet when available. Each link's href is HTML-decoded and passed through an UnwrapRedirect helper; the link text and snippet text are cleaned via CleanText before constructing WebSearchResult instances. The method preserves the original order and stops once the requested `limit` is reached or there are no more link matches.

## Example
```csharp
// html is the raw response body from the DuckDuckGo lite endpoint
var results = ParseLiteEndpoint(html, 10);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} -> {r.Url}\n{r.Snippet}\n");
}
```

## Notes
- Snippet-to-link pairing is positional and can drift if the lite HTML is missing snippets; title and URL are prioritized and are always taken from the link match, so a returned snippet may not always belong to the same URL in malformed documents.
- Results with an empty or whitespace title or URL are skipped, so the returned list may contain fewer than `limit` entries.
- The href is HTML-decoded and then unwrapped via UnwrapRedirect; if UnwrapRedirect yields an empty or invalid string the entry will be skipped.
Parses the HTML returned by DuckDuckGo's "lite" endpoint and extracts up to `limit` search results as WebSearchResult instances. Use this when you have the raw lite HTML and need decoded, cleaned titles and URLs (redirects unwrapped) plus the associated snippet text.

## Remarks
This method depends on two regular expressions (LiteResultLinkRegex and LiteSnippetRegex) to locate link/title pairs and snippet blocks in the lite layout. It HTML-decodes the matched href, cleans the title/snippet text, and runs the href through UnwrapRedirect to get the real URL. Links and snippets are paired by their document order (index), so while the url/title values are taken directly from the link matches, snippet alignment can drift if the counts of matches differ.

## Example
```csharp
// Parse the lite HTML and print the first 5 results
var results = ParseLiteEndpoint(liteHtml, 5);
foreach (var r in results)
{
    Console.WriteLine($"Title: {r.Title}");
    Console.WriteLine($"Url: {r.Url}");
    Console.WriteLine($"Snippet: {r.Snippet}\n");
}
```

## Notes
- If a matched link yields an empty or whitespace title or URL it is skipped; the returned list may therefore contain fewer than `limit` items.
- Snippet matching is position-based: when snippet and link counts diverge the method still returns the correct title/URL but the snippet may be from a different neighboring result.
- The method is static and uses only local variables, so it has no shared state and is safe to call concurrently.
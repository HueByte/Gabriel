Parses an HTML search-results page and extracts up to the specified number of WebSearchResult items.

This method scans the provided HTML using precompiled regular expressions to find individual result blocks, then for each block extracts the title link (decoding HTML entities and unwrapping any redirect), cleans the title and snippet text, and creates a WebSearchResult containing title, url, and snippet. It skips any entry missing a title or URL and stops when the requested limit is reached.

## Remarks
This routine exists as a lightweight, regex-based HTML scraper used when the structured API response is not available or when the search provider returns an HTML page. It centralizes decoding, text-cleaning, and redirect-unwrapping so callers receive normalized WebSearchResult objects without having to parse raw markup.

## Example
```csharp
// Parse up to 5 results from an HTML response string
var results = ParseHtmlEndpoint(htmlResponse, 5);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} -> {r.Url}\n{r.Snippet}");
}
```

## Notes
- The method is regex-driven and therefore brittle: changes in the provider's HTML structure can cause results to be missed. Ensure HtmlResultBlockRegex, HtmlTitleLinkRegex and HtmlSnippetRegex stay in sync with the page markup.
- Entries with an empty or whitespace-only title or URL are ignored; the returned list may contain fewer items than the requested limit.
- The method decodes HTML entities (WebUtility.HtmlDecode) and calls UnwrapRedirect; neither performs network I/O but UnwrapRedirect may return an empty string for non-standard redirects, which will cause that entry to be skipped.
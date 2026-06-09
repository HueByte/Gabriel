Parses an HTML search-results page and extracts up to the specified number of search results as WebSearchResult instances. This method is intended for use when you have the raw HTML returned by DuckDuckGo's results endpoint (or similarly structured markup) and need a lightweight, regex-driven extraction of title, destination URL and snippet.

## Remarks
This is a focused, internal extractor that relies on a set of predefined regular expressions (HtmlResultBlockRegex, HtmlTitleLinkRegex, HtmlSnippetRegex) and helper functions (CleanText, UnwrapRedirect) rather than a full HTML DOM parser. For each matched result block it decodes HTML entities, extracts the link text and href, unwraps any redirect wrapper, cleans the text content, and skips any entries that lack a non-whitespace title or URL. Extraction stops once the requested limit is reached.

## Example
```csharp
// given `html` contains the raw results page
var results = ParseHtmlEndpoint(html, 5);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}\n{r.Snippet}");
}
```

## Notes
- The implementation is regex-based and brittle: changes in the provider's HTML structure can break extraction.  
- Expected named capture groups must be present: result blocks need a "body" group; title matches must expose "href" and "text" groups.  
- The method trims out entries with empty/whitespace titles or URLs and does not perform deduplication or deep URL validation beyond UnwrapRedirect and HtmlDecode.  
- The List is pre-sized to the provided limit and the loop exits early once the limit is reached.
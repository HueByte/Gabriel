Parses raw HTML returned by the DuckDuckGo endpoint and extracts up to `limit` search results as a list of WebSearchResult objects. Use this helper when you have the endpoint's HTML response and need a small, cleaned collection of title/URL/snippet triples rather than working with raw markup.

## Remarks
This is an internal parsing helper that iterates over result blocks matched by `HtmlResultBlockRegex`, then extracts the title link via `HtmlTitleLinkRegex` and an optional snippet via `HtmlSnippetRegex`. Titles are HTML-decoded and cleaned (via `CleanText`) and URLs are passed through `UnwrapRedirect` to remove DuckDuckGo redirect wrappers. Blocks missing a title or a resolved URL are skipped; parsing stops as soon as the requested `limit` is reached.

## Example
```csharp
// Given `html` is the raw HTML returned from DuckDuckGo
var results = ParseHtmlEndpoint(html, 10);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}\n{r.Snippet}");
}
```

## Notes
- The method depends on the regexes exposing named groups "body", "href" and "text": changing those patterns requires updating the consumers here.
- It pre-allocates the result list with the provided `limit` but will still iterate matches until the limit is reached, so extremely large HTML inputs may incur extra work before the break occurs.
- URLs and titles that become empty after decoding/cleaning are silently skipped; callers should expect fewer than `limit` results in that case.
- Because the parsing approach is regex-based and tailored to the current endpoint HTML, it can break if the markup structure changes.
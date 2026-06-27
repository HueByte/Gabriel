Matches a DuckDuckGo search-result snippet cell and captures its inner HTML. Use this when scraping the snippet text from DuckDuckGo result pages where the snippet is rendered inside a <td> element with class='result-snippet'. The regex places the captured content in the named group "text".

## Remarks
The pattern is compiled for runtime performance and uses RegexOptions.Singleline so that the dot (.) matches newlines — necessary because snippet HTML can contain line breaks. The expression expects the class attribute to appear as class='result-snippet' (single quotes) and will capture everything between the opening <td...> and the closing </td> into the "text" group.

## Example
```csharp
var html = "..."; // HTML of a DuckDuckGo results page
var match = LiteSnippetRegex.Match(html);
if (match.Success)
{
    string snippetHtml = match.Groups["text"].Value;
    // snippetHtml contains the inner HTML of the <td class='result-snippet'> element
}
```

## Notes
- The regex is brittle to HTML variations: it expects single quotes for the class attribute and a specific attribute ordering; it will fail if DuckDuckGo changes the markup or uses double quotes.
- The captured value is raw inner HTML (may include tags and entities); you may need to HTML-decode or strip tags depending on your use.
- Regex-on-HTML is inherently fragile — prefer an HTML parser (e.g., HtmlAgilityPack) for robust, long-term scraping.
- The static compiled Regex is safe for concurrent use and improves match performance at the cost of a higher JIT/compile startup cost.
Matches a <td> element whose class attribute is exactly 'result-snippet' and captures the td's inner content into the named group "text". Use this when extracting the snippet HTML from search-result rows in DuckDuckGo's result markup.

## Remarks
This compiled, single-line regex is optimized for repeated use when parsing search-result HTML. RegexOptions.Singleline makes the dot (.) match newlines so the captured group can contain multi-line HTML, and RegexOptions.Compiled improves runtime performance for repeated matches. The named group "text" holds the raw inner HTML (not plain text).

## Example
```csharp
// inside the same class that declares LiteSnippetRegex
var match = LiteSnippetRegex.Match(htmlFragment);
if (match.Success)
{
    string snippetHtml = match.Groups["text"].Value;
    // snippetHtml contains the inner HTML of the <td class='result-snippet'> element
}
```

## Notes
- The pattern requires single quotes around the class value (class='result-snippet'); it will not match if the attribute uses double quotes (class="result-snippet").
- Matching is case-sensitive for the attribute text because no IgnoreCase option is specified.
- The captured group contains raw HTML (including tags and entities); callers should HTML-decode or strip tags if plain text is required.
- Using a regex for HTML parsing is fragile: changes in markup, attribute ordering/style, or additional quoting may cause the pattern to fail.
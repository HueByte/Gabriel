A precompiled regular expression used to extract the inner HTML of a <td> element whose class attribute is exactly 'result-snippet' from DuckDuckGo search result HTML. Use this when scraping the snippet cell from search result rows; the matched content is available in the named capture group "text".

## Remarks
This field exists as a static, compiled Regex to make repeated extraction fast and thread-safe inside the DuckDuckGoWebSearch component. The pattern is deliberately simple and tuned to the specific HTML produced by DuckDuckGo as expected by this codebase, but it is brittle to variations in attribute quoting, attribute order, or changes to DuckDuckGo's markup — consider an HTML parser if the input is not stable.

## Example
```csharp
var match = LiteSnippetRegex.Match(htmlChunk);
if (match.Success)
{
    // innerHtml contains the HTML inside the <td> (may include tags and entities)
    string innerHtml = match.Groups["text"].Value;
    // If you need plain text, decode entities and strip tags here
}
```

## Notes
- The regex requires the class attribute to be written as class='result-snippet' (single quotes) and will not match double-quoted attributes or different attribute ordering.
- The named capture group "text" returns the inner HTML of the <td>, not plain text; callers should HtmlDecode and remove markup if they need raw text.
- RegexOptions.Singleline makes '.' match newlines; RegexOptions.Compiled improves repeated-match performance but has an upfront JIT cost.
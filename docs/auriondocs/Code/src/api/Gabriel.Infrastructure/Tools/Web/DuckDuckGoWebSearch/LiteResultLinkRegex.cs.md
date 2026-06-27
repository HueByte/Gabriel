Matches anchor elements used by DuckDuckGo's "lite" search results and captures the link target and visible text. Reach for this when parsing HTML returned from the lite endpoint to extract result links (the regex specifically looks for <a ... class='result-link'>...</a> and captures the href and inner text).

## Remarks
The lite endpoint uses single-quoted class attributes and a flatter HTML structure than the main endpoint; this regex is tuned to that layout. It defines two named capture groups: "href" (the value of the href attribute, expected to be double-quoted) and "text" (the anchor's inner HTML). RegexOptions.Singleline is used so the text capture can span newlines, and RegexOptions.Compiled improves performance for repeated use.

## Example
```csharp
var match = LiteResultLinkRegex.Match(htmlFragment);
if (match.Success)
{
    var href = match.Groups["href"].Value; // raw href as found in the attribute
    var innerHtml = match.Groups["text"].Value; // anchor inner HTML (may contain tags/newlines)
    // Typically you'd unwrap redirects or HTML-decode/strip HTML from innerHtml before use
}
```

## Notes
- This is not a full HTML parser; the pattern is fragile to changes in attribute quoting, attribute order, or markup variations. It specifically looks for class='result-link' (single quotes) and href="..." (double quotes).
- Because the regex is compiled, there is a small upfront JIT/compile cost but faster matches thereafter.
- The "text" capture contains inner HTML, not plain text — strip tags or HTML-decode as needed before displaying to users.
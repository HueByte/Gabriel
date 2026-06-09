Extracts individual DuckDuckGo HTML "result" blocks from a full search-results page by matching a <div> element whose class contains the token "result" and capturing its inner HTML in a named group `body`. Reach for this when you need a lightweight way to enumerate or slice the server-generated result entries from the standard DuckDuckGo HTML endpoint before further processing.

## Remarks
This compiled, single-line regex is tuned to the server-side structure used by DuckDuckGo: it matches a <div> with a class containing the word `result` and captures everything up to the next result <div>, a sentinel <div id="bottom_spacing">, or the end of the document. Using a readonly, compiled Regex improves match performance for repeated use, and Singleline mode allows the `.` token to span newlines so inner HTML is captured intact.

## Example
```csharp
// Find all result blocks and read each block's inner HTML
var matches = HtmlResultBlockRegex.Matches(htmlContent);
foreach (Match m in matches)
{
    string innerHtml = m.Groups["body"].Value; // raw inner HTML of the result div
    // further parse title, snippet, url hint from innerHtml
}
```

## Notes
- This is not a full HTML parser: changes to DuckDuckGo's markup (class names or structure) can break matching and will result in zero matches.
- The regex captures raw inner HTML; callers must perform additional parsing (e.g., another regex or an HTML parser) to extract fields like title, snippet, and URL.
- RegexOptions.Compiled improves repeat-match performance but incurs a one-time compilation cost at first use.
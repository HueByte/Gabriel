Matches a single DuckDuckGo HTML search-result block and captures its inner content.

This regex finds <div> elements whose class attribute contains the token "result" and captures everything inside that div (named capture group "body") up to the next result div, a div with id "bottom_spacing", or the end of the document. It's intended for the library's HTML endpoint parsing so callers can extract each server-rendered result block for further processing.

## Remarks
The expression is tuned to the server-side HTML shape produced by DuckDuckGo (stable class names and structure). It uses a non-greedy capture and a lookahead that stops at the start of the next result block or a known footer marker; if DuckDuckGo changes their markup the surrounding code falls back to returning zero results and may try the lighter endpoint instead. The RegexOptions.Singleline and RegexOptions.Compiled options are chosen so the dot (.) matches newlines and the compiled regex gives better runtime performance for repeated use.

## Example
```csharp
// Extract each result block's inner HTML
var html = await httpClient.GetStringAsync(url);
foreach (Match m in HtmlResultBlockRegex.Matches(html))
{
    string resultBodyHtml = m.Groups["body"].Value;
    // parse resultBodyHtml to find title, snippet, url hint, etc.
}
```

## Notes
- The regex captures the raw inner HTML for a result block (including tags); further parsing is required to extract title, snippet, or URL.
- Parsing HTML with regex is brittle: this relies on DuckDuckGo's current server-side structure and class names. Expect failures if markup changes.
- RegexOptions.Singleline makes '.' match newlines; RegexOptions.Compiled improves match speed but adds JIT/compile cost at startup.
- The field is static and readonly; Regex instances are thread-safe for concurrent use.